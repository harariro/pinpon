/*
 * StencilController.cs v1.0
 * RealToon Stencil System - ステンシル制御システム
 * 
 * 機能:
 * - See Through機能の有効化/無効化
 * - 前髪越し表現用のステンシル値制御
 * - Writer/Readerマテリアルの自動設定
 * 
 * 関連ファイル:
 * - Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs
 * - Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader
 * 
 * 呼び出し元:
 * - StencilTester.cs (テスト用UI)
 * - DirectCharacterController.cs (統合制御)
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace RealToonStencilSystem
{
    public class StencilController : MonoBehaviour
    {
        #region Properties

        [Header("Stencil System Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool autoEnableSeeThrough = true;

        [Header("Hair-Through-Eye Expression")]
        [SerializeField] private int stencilReferenceValue = 1;
        
        // Static instance for singleton pattern
        private static StencilController _instance;
        public static StencilController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<StencilController>();
                return _instance;
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (autoEnableSeeThrough)
                {
                    EnableSeeThroughFeature();
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            DebugLog("StencilController initialized");
        }

        #endregion

        #region See Through Feature Control

        /// <summary>
        /// See Through機能を有効化
        /// RealToonシェーダーのステンシル機能を有効にする
        /// </summary>
        public bool EnableSeeThroughFeature()
        {
            DebugLog("Attempting to enable See Through feature...");
            
            try
            {
                // Step 1: RealToonGUIスクリプトの変更
                bool guiModified = ModifyRealToonGUI(false); // add_st = false にする
                
                // Step 2: シェーダーファイルの直接修正
                bool shaderModified = ModifyShaderFile(true); // ステンシルブロックを有効化
                
                if (guiModified || shaderModified)
                {
                    DebugLog("See Through feature enabled successfully");
                    
                    // アセットを再インポート
                    EditorApplication.delayCall += () => {
                        string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
                        if (File.Exists(shaderPath))
                        {
                            AssetDatabase.ImportAsset(shaderPath);
                            DebugLog("Shader re-imported");
                        }
                    };
                    
                    return true;
                }
                else
                {
                    DebugLog("See Through feature may already be enabled");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StencilSystem-StencilController] Error enabling See Through: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// See Through機能を無効化
        /// </summary>
        public bool DisableSeeThroughFeature()
        {
            DebugLog("Attempting to disable See Through feature...");
            
            try
            {
                // Step 1: RealToonGUIスクリプトの変更
                bool guiModified = ModifyRealToonGUI(true); // add_st = true にする
                
                // Step 2: シェーダーファイルの直接修正  
                bool shaderModified = ModifyShaderFile(false); // ステンシルブロックを無効化

                if (guiModified || shaderModified)
                {
                    EditorApplication.delayCall += () => {
                        string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
                        if (File.Exists(shaderPath))
                        {
                            AssetDatabase.ImportAsset(shaderPath);
                        }
                    };
                    
                    DebugLog("See Through feature disabled successfully");
                    return true;
                }
                else
                {
                    DebugLog("See Through feature may already be disabled");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StencilSystem-StencilController] Error disabling See Through: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// RealToonGUIスクリプトの変更
        /// </summary>
        /// <param name="enableAddSt">add_st = true にするかどうか</param>
        /// <returns>変更があった場合true</returns>
        private bool ModifyRealToonGUI(bool enableAddSt)
        {
            string guiScriptPath = "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs";
            
            if (!File.Exists(guiScriptPath))
            {
                Debug.LogError($"[StencilSystem-StencilController] RealToon GUI script not found: {guiScriptPath}");
                return false;
            }

            string content = File.ReadAllText(guiScriptPath);
            bool modified = false;

            if (enableAddSt)
            {
                // add_st = false → add_st = true
                if (content.Contains("add_st = false"))
                {
                    content = content.Replace("add_st = false", "add_st = true");
                    modified = true;
                    DebugLog("Changed add_st from false to true");
                }

                if (content.Contains("Remove 'See Through' feature"))
                {
                    content = content.Replace("Remove 'See Through' feature", "Add 'See Through' feature");
                    DebugLog("Updated add_st_string");
                }
            }
            else
            {
                // add_st = true → add_st = false
                if (content.Contains("add_st = true"))
                {
                    content = content.Replace("add_st = true", "add_st = false");
                    modified = true;
                    DebugLog("Changed add_st from true to false");
                }

                if (content.Contains("Add 'See Through' feature"))
                {
                    content = content.Replace("Add 'See Through' feature", "Remove 'See Through' feature");
                    DebugLog("Updated add_st_string");
                }
            }

            if (modified)
            {
                File.WriteAllText(guiScriptPath, content);
                AssetDatabase.ImportAsset(guiScriptPath);
            }

            return modified;
        }

        /// <summary>
        /// シェーダーファイルの直接修正
        /// </summary>
        /// <param name="enableStencil">ステンシルを有効化するかどうか</param>
        /// <returns>変更があった場合true</returns>
        private bool ModifyShaderFile(bool enableStencil)
        {
            string shaderPath = "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader";
            
            DebugLog($"ModifyShaderFile called - enableStencil: {enableStencil}");
            
            if (!File.Exists(shaderPath))
            {
                Debug.LogError($"[StencilSystem-StencilController] Shader file not found: {shaderPath}");
                return false;
            }

            string content = File.ReadAllText(shaderPath);
            string originalContent = content;
            bool modified = false;

            DebugLog($"Shader file loaded - size: {content.Length} characters");

            if (enableStencil)
            {
                DebugLog("Attempting to enable stencil blocks...");
                
                // ステンシルブロックを有効化
                // ForwardLitパス
                if (content.Contains("/*//F_ST"))
                {
                    content = content.Replace("/*//F_ST", "//F_ST");
                    content = content.Replace("//F_ST_En*/", "//F_ST_En");
                    modified = true;
                    DebugLog("Enabled ForwardLit stencil block");
                }
                else
                {
                    DebugLog("ForwardLit stencil block already enabled or not found in expected format");
                }

                // Outlineパス  
                if (content.Contains("/*//O_ST"))
                {
                    content = content.Replace("/*//O_ST", "//O_ST");
                    content = content.Replace("//O_ST_En*/", "//O_ST_En");
                    modified = true;
                    DebugLog("Enabled Outline stencil block");
                }
                else
                {
                    DebugLog("Outline stencil block already enabled or not found in expected format");
                }

                // GBufferパス
                if (content.Contains("/*//G_ST"))
                {
                    content = content.Replace("/*//G_ST", "//G_ST");  
                    content = content.Replace("//G_ST_En*/", "//G_ST_En");
                    modified = true;
                    DebugLog("Enabled GBuffer stencil block");
                }
                else
                {
                    DebugLog("GBuffer stencil block already enabled or not found in expected format");
                }
            }
            else
            {
                DebugLog("Attempting to disable stencil blocks...");
                
                // ステンシルブロックを無効化
                // ForwardLitパス
                if (content.Contains("//F_ST") && !content.Contains("/*//F_ST"))
                {
                    content = content.Replace("//F_ST", "/*//F_ST");
                    content = content.Replace("//F_ST_En", "//F_ST_En*/");
                    modified = true;
                    DebugLog("Disabled ForwardLit stencil block");
                }
                else
                {
                    DebugLog("ForwardLit stencil block already disabled or not found in expected format");
                }

                // Outlineパス
                if (content.Contains("//O_ST") && !content.Contains("/*//O_ST"))
                {
                    content = content.Replace("//O_ST", "/*//O_ST");
                    content = content.Replace("//O_ST_En", "//O_ST_En*/");
                    modified = true;
                    DebugLog("Disabled Outline stencil block");
                }
                else
                {
                    DebugLog("Outline stencil block already disabled or not found in expected format");
                }

                // GBufferパス  
                if (content.Contains("//G_ST") && !content.Contains("/*//G_ST"))
                {
                    content = content.Replace("//G_ST", "/*//G_ST");
                    content = content.Replace("//G_ST_En", "//G_ST_En*/");
                    modified = true;
                    DebugLog("Disabled GBuffer stencil block");
                }
                else
                {
                    DebugLog("GBuffer stencil block already disabled or not found in expected format");
                }
            }

            if (modified)
            {
                try
                {
                    File.WriteAllText(shaderPath, content);
                    DebugLog($"Shader file modified successfully - Stencil {(enableStencil ? "enabled" : "disabled")}");
                    DebugLog($"Content changed from {originalContent.Length} to {content.Length} characters");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[StencilSystem-StencilController] Failed to write shader file: {e.Message}");
                    return false;
                }
            }
            else
            {
                DebugLog("No modifications were made to the shader file");
            }

            return modified;
        }

        #endregion

        #region Material Configuration

        /// <summary>
        /// 髪用マテリアルをReader設定に変更
        /// ステンシル値を読み取り、透過処理を行う
        /// </summary>
        /// <param name="material">対象マテリアル</param>
        public void ConfigureHairMaterial(Material material)
        {
            if (material == null)
            {
                Debug.LogError("[StencilSystem-StencilController] Material is null");
                return;
            }

            if (!IsRealToonMaterial(material))
            {
                Debug.LogError("[StencilSystem-StencilController] Material is not RealToon shader");
                return;
            }

            // Hair用設定 (Reader) - RealToon仕様
            material.SetInt("_RefVal", stencilReferenceValue);  // 参照値: 1
            material.SetInt("_Oper", 0);   // A = Reader設定
            material.SetInt("_Compa", 7);  // B = ステンシル値と一致しない場合に描画
            
            DebugLog($"Configured Hair material: {material.name} as Reader (RefVal={stencilReferenceValue}, Oper=A(0), Compa=B(7))");
        }

        /// <summary>
        /// 眉毛・目用マテリアルをWriter設定に変更
        /// ステンシル値をバッファに書き込む
        /// </summary>
        /// <param name="material">対象マテリアル</param>
        public void ConfigureEyeFaceMaterial(Material material)
        {
            if (material == null)
            {
                Debug.LogError("[StencilSystem-StencilController] Material is null");
                return;
            }

            if (!IsRealToonMaterial(material))
            {
                Debug.LogError("[StencilSystem-StencilController] Material is not RealToon shader");
                return;
            }

            // Eye/Face用設定 (Writer) - RealToon仕様
            material.SetInt("_RefVal", stencilReferenceValue);  // 参照値: 1
            material.SetInt("_Oper", 2);   // B = Writer設定  
            material.SetInt("_Compa", 6);  // A = 常に描画
            
            DebugLog($"Configured Eye/Face material: {material.name} as Writer (RefVal={stencilReferenceValue}, Oper=B(2), Compa=A(6))");
        }

        /// <summary>
        /// RealToonマテリアルかどうかチェック
        /// </summary>
        /// <param name="material">チェック対象マテリアル</param>
        /// <returns>RealToonマテリアルの場合true</returns>
        private bool IsRealToonMaterial(Material material)
        {
            return material.shader.name.Contains("RealToon");
        }

        #endregion

        #region Stencil Value Control

        /// <summary>
        /// ステンシル参照値を変更
        /// </summary>
        /// <param name="newValue">新しい参照値 (0-255)</param>
        public void SetStencilReferenceValue(int newValue)
        {
            if (newValue < 0 || newValue > 255)
            {
                Debug.LogError("[StencilSystem-StencilController] Stencil value must be between 0-255");
                return;
            }

            stencilReferenceValue = newValue;
            DebugLog($"Stencil reference value set to: {newValue}");
        }

        /// <summary>
        /// 現在のステンシル参照値を取得
        /// </summary>
        /// <returns>現在の参照値</returns>
        public int GetStencilReferenceValue()
        {
            return stencilReferenceValue;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[StencilSystem-StencilController] {message}");
            }
        }

        /// <summary>
        /// See Through機能の現在の状態を取得
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsSeeThroughFeatureEnabled()
        {
            try
            {
                string guiScriptPath = "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs";
                if (!File.Exists(guiScriptPath)) return false;

                string content = File.ReadAllText(guiScriptPath);
                
                // add_st = false の場合、See Through機能は有効
                bool isEnabled = content.Contains("add_st = false");
                DebugLog($"See Through status check: {(isEnabled ? "ENABLED" : "DISABLED")}");
                
                return isEnabled;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StencilSystem-StencilController] Error checking See Through status: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Phase 2 Placeholder

        // Phase 2で実装予定の高度な制御機能

        /// <summary>
        /// レンダーキューの動的制御（Phase 2実装予定）
        /// </summary>
        /// <param name="material">対象マテリアル</param>
        /// <param name="queueValue">キュー値</param>
        public void SetRenderQueue(Material material, int queueValue)
        {
            DebugLog("SetRenderQueue feature will be implemented in Phase 2");
            // Phase 2で実装
        }

        /// <summary>
        /// プリセット管理（Phase 2実装予定）
        /// </summary>
        /// <param name="presetName">プリセット名</param>
        public void ApplyStencilPreset(string presetName)
        {
            DebugLog("ApplyStencilPreset feature will be implemented in Phase 2");
            // Phase 2で実装
        }

        #endregion
    }
}
#endif