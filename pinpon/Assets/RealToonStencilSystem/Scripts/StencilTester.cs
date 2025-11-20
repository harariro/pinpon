/*
 * StencilTester.cs v1.1 - 診断機能付き
 * RealToon Stencil System - テスト・検証用UI
 * 
 * v1.1追加機能:
 * - Check Shader Status: シェーダーファイルの詳細診断
 * - Force Shader Fix: 強制修正機能
 * - 詳細なデバッグ情報表示
 * 
 * 機能:
 * - See Through機能の有効化/無効化テスト
 * - マテリアル設定のテスト
 * - ステンシル値の動的変更テスト
 * - デバッグ情報の表示
 * - シェーダーファイル診断
 * 
 * 関連ファイル:
 * - StencilController.cs
 * 
 * 使用方法:
 * - シーンに空のGameObjectを作成
 * - このスクリプトをアタッチ
 * - Playモードでテスト実行
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace RealToonStencilSystem
{
    public class StencilTester : MonoBehaviour
    {
        #region Inspector Properties

        [Header("Test Configuration")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private bool autoInitialize = true;
        
        [Header("Test Materials")]
        [SerializeField] private Material hairMaterial;
        [SerializeField] private Material eyeFaceMaterial;
        
        [Header("Test Values")]
        [SerializeField] private int testStencilValue = 1;
        [SerializeField] private bool enableDebugLogs = true;

        private StencilController stencilController;
        private bool showGUI = true;
        private Vector2 scrollPosition;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (autoInitialize)
            {
                InitializeStencilController();
            }
        }

        private void Start()
        {
            DebugLog("StencilTester v1.1 initialized with diagnostic tools");
            if (showDebugUI)
            {
                DebugLog("Debug UI enabled - Press F1 to toggle GUI");
            }
        }

        private void Update()
        {
            // F1キーでUI表示切り替え
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showGUI = !showGUI;
                DebugLog($"Debug GUI {(showGUI ? "enabled" : "disabled")}");
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// StencilControllerの初期化
        /// </summary>
        private void InitializeStencilController()
        {
            // 既存のStencilControllerを探す
            stencilController = FindObjectOfType<StencilController>();
            
            if (stencilController == null)
            {
                // 新しいGameObjectにStencilControllerを追加
                GameObject controllerObj = new GameObject("StencilController");
                stencilController = controllerObj.AddComponent<StencilController>();
                DebugLog("StencilController created automatically");
            }
            else
            {
                DebugLog("Found existing StencilController");
            }
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (!showGUI || !showDebugUI) return;

            // GUI スタイル設定
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            // メインウィンドウ
            GUILayout.BeginArea(new Rect(10, 10, 450, Screen.height - 20));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, boxStyle);

            // タイトル
            GUILayout.Label("RealToon Stencil System Tester v1.1", titleStyle);
            GUILayout.Space(10);

            // See Through機能制御
            DrawSeeThroughControls();
            GUILayout.Space(10);

            // マテリアル設定テスト
            DrawMaterialTests();
            GUILayout.Space(10);

            // ステンシル値制御
            DrawStencilValueControls();
            GUILayout.Space(10);

            // 診断ツール
            DrawDiagnosticTools();
            GUILayout.Space(10);

            // 状態情報表示
            DrawStatusInfo();
            GUILayout.Space(10);

            // ユーティリティ
            DrawUtilities();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// See Through機能制御UI
        /// </summary>
        private void DrawSeeThroughControls()
        {
            GUILayout.Label("See Through Feature Control", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable See Through"))
            {
                if (stencilController != null)
                {
                    bool result = stencilController.EnableSeeThroughFeature();
                    DebugLog($"Enable See Through result: {result}");
                }
                else
                {
                    DebugLog("StencilController not found!");
                }
            }
            
            if (GUILayout.Button("Disable See Through"))
            {
                if (stencilController != null)
                {
                    bool result = stencilController.DisableSeeThroughFeature();
                    DebugLog($"Disable See Through result: {result}");
                }
                else
                {
                    DebugLog("StencilController not found!");
                }
            }
            GUILayout.EndHorizontal();

            // 現在の状態表示
            if (stencilController != null)
            {
                bool isEnabled = stencilController.IsSeeThroughFeatureEnabled();
                GUI.color = isEnabled ? Color.green : Color.red;
                GUILayout.Label($"Status: {(isEnabled ? "ENABLED" : "DISABLED")}");
                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// マテリアル設定テストUI
        /// </summary>
        private void DrawMaterialTests()
        {
            GUILayout.Label("Material Configuration Tests", EditorStyles.boldLabel);
            
            // Hair Material
            GUILayout.BeginHorizontal();
            GUILayout.Label("Hair Material:", GUILayout.Width(100));
            hairMaterial = (Material)EditorGUILayout.ObjectField(hairMaterial, typeof(Material), false);
            if (GUILayout.Button("Configure as Reader", GUILayout.Width(150)))
            {
                if (hairMaterial != null && stencilController != null)
                {
                    stencilController.ConfigureHairMaterial(hairMaterial);
                    DebugLog($"Configured {hairMaterial.name} as Hair Reader");
                }
            }
            GUILayout.EndHorizontal();

            // Eye/Face Material
            GUILayout.BeginHorizontal();
            GUILayout.Label("Eye/Face Material:", GUILayout.Width(100));
            eyeFaceMaterial = (Material)EditorGUILayout.ObjectField(eyeFaceMaterial, typeof(Material), false);
            if (GUILayout.Button("Configure as Writer", GUILayout.Width(150)))
            {
                if (eyeFaceMaterial != null && stencilController != null)
                {
                    stencilController.ConfigureEyeFaceMaterial(eyeFaceMaterial);
                    DebugLog($"Configured {eyeFaceMaterial.name} as Eye/Face Writer");
                }
            }
            GUILayout.EndHorizontal();

            // Quick Setup
            if (GUILayout.Button("Quick Setup Both Materials"))
            {
                if (hairMaterial != null && eyeFaceMaterial != null && stencilController != null)
                {
                    stencilController.ConfigureHairMaterial(hairMaterial);
                    stencilController.ConfigureEyeFaceMaterial(eyeFaceMaterial);
                    DebugLog("Quick setup completed for both materials");
                }
                else
                {
                    DebugLog("Please assign both materials first");
                }
            }
        }

        /// <summary>
        /// ステンシル値制御UI
        /// </summary>
        private void DrawStencilValueControls()
        {
            GUILayout.Label("Stencil Value Controls", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Test Stencil Value:", GUILayout.Width(120));
            testStencilValue = EditorGUILayout.IntSlider(testStencilValue, 0, 255);
            if (GUILayout.Button("Apply", GUILayout.Width(60)))
            {
                if (stencilController != null)
                {
                    stencilController.SetStencilReferenceValue(testStencilValue);
                    DebugLog($"Set stencil reference value to: {testStencilValue}");
                }
            }
            GUILayout.EndHorizontal();

            if (stencilController != null)
            {
                int currentValue = stencilController.GetStencilReferenceValue();
                GUILayout.Label($"Current Value: {currentValue}");
            }

            // プリセット値ボタン
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Value: 0")) SetStencilValue(0);
            if (GUILayout.Button("Value: 1")) SetStencilValue(1);
            if (GUILayout.Button("Value: 2")) SetStencilValue(2);
            if (GUILayout.Button("Value: 255")) SetStencilValue(255);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 診断ツールUI
        /// </summary>
        private void DrawDiagnosticTools()
        {
            GUILayout.Label("Diagnostic Tools", EditorStyles.boldLabel);
            
            GUI.color = Color.cyan;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Check Shader Status"))
            {
                CheckShaderFileStatus();
            }
            if (GUILayout.Button("🔧 Force Shader Fix"))
            {
                ForceShaderFix();
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            // 修正ツール
            GUI.color = Color.yellow;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🛠️ Fix Outline Stencil"))
            {
                FixOutlineStencilStructure();
            }
            if (GUILayout.Button("🔄 Reimport Shader"))
            {
                ReimportShaderFile();
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("📄 Show Shader Path"))
            {
                ShowShaderFilePath();
            }
            if (GUILayout.Button("🧪 Test Stencil Effect"))
            {
                TestStencilEffect();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 状態情報表示UI
        /// </summary>
        private void DrawStatusInfo()
        {
            GUILayout.Label("System Status", EditorStyles.boldLabel);
            
            // StencilController状態
            if (stencilController != null)
            {
                GUI.color = Color.green;
                GUILayout.Label("✓ StencilController: Active");
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("✗ StencilController: Missing");
            }
            GUI.color = Color.white;

            // マテリアル状態
            GUI.color = hairMaterial != null ? Color.green : Color.red;
            GUILayout.Label($"{(hairMaterial != null ? "✓" : "✗")} Hair Material: {(hairMaterial != null ? hairMaterial.name : "Not Assigned")}");
            
            GUI.color = eyeFaceMaterial != null ? Color.green : Color.red;
            GUILayout.Label($"{(eyeFaceMaterial != null ? "✓" : "✗")} Eye/Face Material: {(eyeFaceMaterial != null ? eyeFaceMaterial.name : "Not Assigned")}");
            GUI.color = Color.white;

            // Phase情報
            GUILayout.Label("Current Phase: Phase 1 - Basic Stencil Control");
        }

        /// <summary>
        /// ユーティリティUI
        /// </summary>
        private void DrawUtilities()
        {
            GUILayout.Label("Utilities", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Console"))
            {
                System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor))
                    .GetType("UnityEditor.LogEntries")
                    .GetMethod("Clear")
                    .Invoke(new object(), null);
                DebugLog("Console cleared");
            }
            
            if (GUILayout.Button("Refresh Materials"))
            {
                AssetDatabase.Refresh();
                DebugLog("Asset database refreshed");
            }
            GUILayout.EndHorizontal();

            // デバッグログ制御
            GUILayout.BeginHorizontal();
            enableDebugLogs = GUILayout.Toggle(enableDebugLogs, "Enable Debug Logs");
            if (GUILayout.Button("Test Log"))
            {
                DebugLog("This is a test debug message");
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Press F1 to toggle this GUI", EditorStyles.miniLabel);
        }

        #endregion

        #region Diagnostic Methods

        /// <summary>
        /// シェーダーファイルの状態をチェック
        /// </summary>
        private void CheckShaderFileStatus()
        {
            DebugLog("=== Shader File Diagnostic START ===");
            
            string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
            if (!System.IO.File.Exists(shaderPath))
            {
                DebugLog("ERROR: Shader file not found!");
                DebugLog($"Expected path: {shaderPath}");
                return;
            }

            DebugLog($"Shader file found: {shaderPath}");
            
            string content = System.IO.File.ReadAllText(shaderPath);
            DebugLog($"File size: {content.Length} characters");
            
            // ForwardLit パスのチェック
            bool forwardLitCommentedOut = content.Contains("/*//F_ST");
            bool forwardLitActive = content.Contains("//F_ST\n") || content.Contains("//F_ST\r\n");
            bool forwardLitRealToonActive = content.Contains("//F_ST/*");
            
            DebugLog($"ForwardLit Analysis:");
            DebugLog($"  - Contains /*//F_ST: {forwardLitCommentedOut}");
            DebugLog($"  - Contains //F_ST (newline): {forwardLitActive}");
            DebugLog($"  - Contains //F_ST/*: {forwardLitRealToonActive}");
            
            string forwardLitStatus = "UNKNOWN";
            if (forwardLitCommentedOut) forwardLitStatus = "DISABLED (/*//F_ST format)";
            else if (forwardLitRealToonActive) forwardLitStatus = "ENABLED (//F_ST/* format)";
            else if (forwardLitActive) forwardLitStatus = "ENABLED (//F_ST format)";
            
            DebugLog($"ForwardLit Stencil: {forwardLitStatus}");
            
            // Outline パスのチェック  
            bool outlineCommentedOut = content.Contains("/*//O_ST");
            bool outlineActive = content.Contains("//O_ST\n") || content.Contains("//O_ST\r\n");
            bool outlineRealToonActive = content.Contains("//O_ST/*");
            
            string outlineStatus = "UNKNOWN";
            if (outlineCommentedOut) outlineStatus = "DISABLED (/*//O_ST format)";
            else if (outlineRealToonActive) outlineStatus = "ENABLED (//O_ST/* format)";
            else if (outlineActive) outlineStatus = "ENABLED (//O_ST format)";
            
            DebugLog($"Outline Stencil: {outlineStatus}");
            
            // GBuffer パスのチェック
            bool gbufferCommentedOut = content.Contains("/*//G_ST");
            bool gbufferActive = content.Contains("//G_ST\n") || content.Contains("//G_ST\r\n");
            bool gbufferRealToonActive = content.Contains("//G_ST/*");
            
            string gbufferStatus = "UNKNOWN";
            if (gbufferCommentedOut) gbufferStatus = "DISABLED (/*//G_ST format)";
            else if (gbufferRealToonActive) gbufferStatus = "ENABLED (//G_ST/* format)";
            else if (gbufferActive) gbufferStatus = "ENABLED (//G_ST format)";
            
            DebugLog($"GBuffer Stencil: {gbufferStatus}");

            // サンプルコード表示
            ShowStencilSamples(content);

            DebugLog("=== Shader File Diagnostic END ===");
        }

        /// <summary>
        /// ステンシルブロックのサンプルを表示
        /// </summary>
        private void ShowStencilSamples(string content)
        {
            DebugLog("=== Stencil Block Samples ===");
            
            // ForwardLit サンプル
            int forwardIndex = content.IndexOf("//F_ST");
            if (forwardIndex == -1) forwardIndex = content.IndexOf("/*//F_ST");
            if (forwardIndex != -1)
            {
                int endIndex = content.IndexOf("//F_ST_En", forwardIndex);
                if (endIndex == -1) endIndex = content.IndexOf("//F_ST_En*/", forwardIndex);
                if (endIndex != -1)
                {
                    int sampleEnd = Math.Min(endIndex + 20, content.Length);
                    string sample = content.Substring(forwardIndex, sampleEnd - forwardIndex);
                    DebugLog($"ForwardLit Sample: {sample.Replace("\n", "\\n").Replace("\r", "\\r")}");
                }
            }
            
            // Outline サンプル  
            int outlineIndex = content.IndexOf("//O_ST");
            if (outlineIndex == -1) outlineIndex = content.IndexOf("/*//O_ST");
            if (outlineIndex != -1)
            {
                int endIndex = content.IndexOf("//O_ST_En", outlineIndex);
                if (endIndex == -1) endIndex = content.IndexOf("//O_ST_En*/", outlineIndex);
                if (endIndex != -1)
                {
                    int sampleEnd = Math.Min(endIndex + 20, content.Length);
                    string sample = content.Substring(outlineIndex, sampleEnd - outlineIndex);
                    DebugLog($"Outline Sample: {sample.Replace("\n", "\\n").Replace("\r", "\\r")}");
                }
            }
        }

        /// <summary>
        /// 強制的にシェーダーを修正
        /// </summary>
        private void ForceShaderFix()
        {
            DebugLog("=== Force Shader Fix START ===");
            
            if (stencilController != null)
            {
                // 一度無効化してから有効化
                DebugLog("Step 1: Disabling See Through feature");
                stencilController.DisableSeeThroughFeature();
                
                System.Threading.Thread.Sleep(100);
                
                DebugLog("Step 2: Enabling See Through feature");
                stencilController.EnableSeeThroughFeature();
                
                DebugLog("Force shader fix completed");
            }
            else
            {
                DebugLog("ERROR: StencilController not found!");
            }
            
            DebugLog("=== Force Shader Fix END ===");
        }

        /// <summary>
        /// シェーダーファイルパスを表示
        /// </summary>
        private void ShowShaderFilePath()
        {
            string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
            DebugLog($"Shader file path: {shaderPath}");
            DebugLog($"File exists: {System.IO.File.Exists(shaderPath)}");
        }

        /// <summary>
        /// Outlineパスのステンシル構造を修正
        /// </summary>
        private void FixOutlineStencilStructure()
        {
            DebugLog("=== Fix Outline Stencil Structure START ===");
            
            string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
            if (!System.IO.File.Exists(shaderPath))
            {
                DebugLog("ERROR: Shader file not found!");
                return;
            }

            string content = System.IO.File.ReadAllText(shaderPath);
            string originalContent = content;
            
            // Outlineパスの不正な構造を修正
            // パターン: //O_ST\r\n\t\t\tRef[_RefVal]\r\n\t\t\tComp [_Compa]\r\n\t\t\tPass [_Oper]\r\n\t\t\tFail [_Oper]\r\n//O_ST_En
            // 正しい形: //O_ST\r\n\t\t\tStencil {\r\n\t\t\t\tRef[_RefVal]\r\n\t\t\t\tComp [_Compa]\r\n\t\t\t\tPass [_Oper]\r\n\t\t\t\tFail [_Oper]\r\n\t\t\t}\r\n//O_ST_En
            
            int outlineIndex = content.IndexOf("//O_ST");
            if (outlineIndex != -1)
            {
                int endIndex = content.IndexOf("//O_ST_En", outlineIndex);
                if (endIndex != -1)
                {
                    string outlineSection = content.Substring(outlineIndex, endIndex - outlineIndex + 10);
                    DebugLog($"Found Outline section: {outlineSection.Replace("\r", "\\r").Replace("\n", "\\n")}");
                    
                    // 不正な構造をチェック（Stencil {がない場合）
                    if (!outlineSection.Contains("Stencil {") && outlineSection.Contains("Ref[_RefVal]"))
                    {
                        DebugLog("Outline stencil structure is incorrect - fixing...");
                        
                        // 正しい構造に置き換え
                        string fixedSection = @"//O_ST
		Stencil {
			Ref[_RefVal]
			Comp [_Compa]
			Pass [_Oper]
			Fail [_Oper]
		}
//O_ST_En";
                        
                        content = content.Replace(outlineSection, fixedSection);
                        
                        try
                        {
                            System.IO.File.WriteAllText(shaderPath, content);
                            DebugLog("Outline stencil structure fixed successfully!");
                            
                            // シェーダーを再インポート
                            AssetDatabase.ImportAsset(shaderPath);
                            DebugLog("Shader re-imported");
                            
                        }
                        catch (System.Exception e)
                        {
                            DebugLog($"ERROR: Failed to write shader file: {e.Message}");
                        }
                    }
                    else
                    {
                        DebugLog("Outline stencil structure appears to be correct");
                    }
                }
                else
                {
                    DebugLog("Could not find //O_ST_En marker");
                }
            }
            else
            {
                DebugLog("Could not find //O_ST marker");
            }
            
            DebugLog("=== Fix Outline Stencil Structure END ===");
        }

        /// <summary>
        /// ステンシル効果のテスト
        /// </summary>
        private void TestStencilEffect()
        {
            DebugLog("=== Stencil Effect Test START ===");
            
            if (hairMaterial == null || eyeFaceMaterial == null)
            {
                DebugLog("ERROR: Both Hair and Eye/Face materials must be assigned!");
                return;
            }

            // マテリアルの現在の設定を確認
            int hairRefVal = hairMaterial.GetInt("_RefVal");
            int hairOper = hairMaterial.GetInt("_Oper");
            int hairCompa = hairMaterial.GetInt("_Compa");
            
            int eyeRefVal = eyeFaceMaterial.GetInt("_RefVal");
            int eyeOper = eyeFaceMaterial.GetInt("_Oper");
            int eyeCompa = eyeFaceMaterial.GetInt("_Compa");
            
            DebugLog($"Hair Material Settings: RefVal={hairRefVal}, Oper={hairOper}, Compa={hairCompa}");
            DebugLog($"Eye/Face Material Settings: RefVal={eyeRefVal}, Oper={eyeOper}, Compa={eyeCompa}");
            
            // 期待される設定と比較
            bool hairCorrect = (hairRefVal == 1 && hairOper == 0 && hairCompa == 7);
            bool eyeCorrect = (eyeRefVal == 1 && eyeOper == 2 && eyeCompa == 6);
            
            DebugLog($"Hair Material Configuration: {(hairCorrect ? "CORRECT" : "INCORRECT")}");
            DebugLog($"Eye/Face Material Configuration: {(eyeCorrect ? "CORRECT" : "INCORRECT")}");
            
            if (hairCorrect && eyeCorrect)
            {
                DebugLog("✅ All material settings are correct for hair-through-eye effect");
            }
            else
            {
                DebugLog("❌ Material settings need correction. Run 'Quick Setup Both Materials' first.");
            }
            
            // レンダーキューの確認
            int hairQueue = hairMaterial.renderQueue;
            int eyeQueue = eyeFaceMaterial.renderQueue;
            
            DebugLog($"Render Queue - Hair: {hairQueue}, Eye/Face: {eyeQueue}");
            
            if (eyeQueue <= hairQueue)
            {
                DebugLog("✅ Render queue order is correct (Eye/Face before Hair)");
            }
            else
            {
                DebugLog("❌ Render queue order may be incorrect (Hair should render after Eye/Face)");
            }
            
            DebugLog("=== Stencil Effect Test END ===");
        }

        /// <summary>
        /// シェーダーファイルを再インポート
        /// </summary>
        private void ReimportShaderFile()
        {
            string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
            if (System.IO.File.Exists(shaderPath))
            {
                AssetDatabase.ImportAsset(shaderPath);
                DebugLog("Shader file re-imported successfully");
            }
            else
            {
                DebugLog("ERROR: Shader file not found for re-import");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ステンシル値を設定する
        /// </summary>
        /// <param name="value">設定値</param>
        private void SetStencilValue(int value)
        {
            testStencilValue = value;
            if (stencilController != null)
            {
                stencilController.SetStencilReferenceValue(value);
                DebugLog($"Set stencil value to: {value}");
            }
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[StencilSystem-StencilTester] {message}");
            }
        }

        #endregion

        #region Context Menu

        [ContextMenu("Initialize Stencil Controller")]
        private void InitializeStencilControllerMenu()
        {
            InitializeStencilController();
        }

        [ContextMenu("Test See Through Enable")]
        private void TestSeeThroughEnable()
        {
            if (stencilController != null)
            {
                bool result = stencilController.EnableSeeThroughFeature();
                DebugLog($"Test Enable See Through result: {result}");
            }
        }

        [ContextMenu("Test See Through Disable")]
        private void TestSeeThroughDisable()
        {
            if (stencilController != null)
            {
                bool result = stencilController.DisableSeeThroughFeature();
                DebugLog($"Test Disable See Through result: {result}");
            }
        }

        [ContextMenu("Run Full Diagnostic")]
        private void RunFullDiagnostic()
        {
            CheckShaderFileStatus();
        }

        #endregion
    }
}
#endif