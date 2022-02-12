#nullable enable

using System.Collections;
using UnityEngine;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

using Edanoue.SceneTest;

namespace Edanoue.SceneTest
{
    /// <summary>
    /// テストケース として振る舞う拡張 MonoBehaviour クラス
    /// </summary>
    public abstract class EdaTestBehaviour : MonoBehaviour, ITestCase
    {
        #region IEdaTestCase

        bool ITestCase.IsRunning => this._isRunning;
        void ITestCase.OnRun() => this.OnRun();
        void ITestCase.OnCancel() => this.OnCancel();
        void ITestCase.OnTimeout() => this.OnTimeout();
        ITestResult ITestCase.Report => this._testReport;
        CaseOptions ITestCase.Options => this._localOptions;

        #endregion

        #region Unity 公開 Property

        /// <summary>
        /// Inspector から設定できるテスト名
        /// 指定されていない場合クラス名が使用される
        /// </summary>
        [SerializeField]
        private string m_customTestName = "";


        /// <summary>
        /// Inspector から設定できるこのテスト独自のTimeout時間
        /// Runner 側に指定されている時間より短い場合, こちらの時間が使用されます
        /// <summary>
        [SerializeField]
        private float m_timeoutSeconds = 10f;

        #endregion

        /// <summary>
        /// 継承先でオーバーライドできるテスト名
        /// デフォルトではクラス名を使用する
        /// </summary>
        protected virtual string TestName => m_customTestName == "" ? this.GetType().Name : m_customTestName;

        // 成功したときのコールバック, ここでは空にしておく
        protected virtual void OnSuccess() { }

        // 失敗したときのコールバック, ここでは空にしておく
        protected virtual void OnFail() { }

        /// <summary>
        /// Runner により呼ばれる タイムアウトされたときのコールバック
        /// </summary>
        protected virtual void OnTimeout()
        {
            // デフォルトではタイムアウトの場合は Fail する
            Fail("Timeouted");
        }

        #region 内部処理用

        bool _isRunning;
        SceneTestCaseResult? _testReport;
        CaseOptions _localOptions;
        SceneTestCase? _testcase;

        /// <summary>
        /// Runner により呼ばれるテスト開始のコールバック
        /// </summary>
        private void OnRun()
        {
            // すでに テストレポートが生成されてたら無視する
            // Runner からの実行前に Awake などで呼ばれているパターン
            if (_testReport is not null)
            {
                return;
            }

            if (!_isRunning)
            {
                _testcase = new("まああ");
                _testReport = new SceneTestCaseResult(_testcase);

                // 実行時点で Inspector に設定されているものからオプションを作成する
                _localOptions = new(
                    localTimeoutSeconds: m_timeoutSeconds
                );

                Debug.Log($"Run {_testReport!.FullName}", this);
                _isRunning = true;
            }
        }

        /// <summary>
        /// Runner により呼ばれるテストキャンセルのコールバック
        /// </summary>
        private void OnCancel()
        {
            if (_isRunning)
            {
                _testReport!.SetResult(ResultState.Cancelled, "Manually canceled");
                _isRunning = false;
            }
        }

        /// <summary>
        /// このテストを成功扱いとする
        /// Runner によりテストが実行されていない状態であっても先に結果の代入は行える
        /// </summary>
        /// <param name="message"></param>
        protected void Success(string? message)
        {
            // Awake などで実行されたとき
            if (_testcase is null)
            {
                _testcase = new("まああ");
                _testReport = new SceneTestCaseResult(_testcase);
            }
            // 結果を代入する
            _testReport.SetResult(ResultState.Success, message);
            _isRunning = false;
        }

        /// <summary>
        /// このテストを失敗扱いとする
        /// Runner によりテストが実行されていない状態であっても先に結果の代入は行える
        /// </summary>
        /// <param name="message"></param>
        protected void Fail(string? message)
        {
            // Awake などで実行されたとき
            if (_testcase is null)
            {
                _testcase = new("まああ");
                _testReport = new SceneTestCaseResult(_testcase);
            }
            // 結果を代入する
            _testReport.SetResult(ResultState.Failure, message);
            _isRunning = false;
        }

        #endregion
    }
}
