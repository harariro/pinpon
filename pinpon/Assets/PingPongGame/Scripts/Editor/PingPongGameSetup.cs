// using UnityEngine;
// using UnityEditor;

// namespace PingPongGame.Editor
// {
//     /// <summary>
//     /// 卓球ゲームのセットアップを簡単にするエディタスクリプト
//     /// </summary>
//     public class PingPongGameSetup : EditorWindow
//     {
//         [MenuItem("PingPong/Setup Game Scene")]
//         public static void SetupGameScene()
//         {
//             if (EditorUtility.DisplayDialog("Setup Ping Pong Game",
//                 "現在のシーンに卓球ゲームの基本オブジェクトを配置しますか？",
//                 "はい", "キャンセル"))
//             {
//                 CreateGameObjects();
//             }
//         }

//         [MenuItem("PingPong/Clear Game Scene")]
//         public static void ClearGameScene()
//         {
//             if (EditorUtility.DisplayDialog("Clear Ping Pong Game",
//                 "卓球ゲームの全オブジェクトを削除しますか？\n（カメラは削除されません）",
//                 "はい", "キャンセル"))
//             {
//                 ClearAllGameObjects();
//             }
//         }

//         private static void ClearAllGameObjects()
//         {
//             int deletedCount = 0;

//             // 削除対象のオブジェクト名リスト
//             string[] objectNames = new string[]
//             {
//                 "GameManager",
//                 "PingPongTable",
//                 "Ball",
//                 "PlayerRacket",
//                 "AIOpponent",
//                 "HitPointManager",
//                 "UIManager",
//                 "TimingGaugeUI",
//                 "FeedbackUI"
//             };

//             foreach (string objName in objectNames)
//             {
//                 GameObject obj = GameObject.Find(objName);
//                 if (obj != null)
//                 {
//                     Object.DestroyImmediate(obj);
//                     deletedCount++;
//                     Debug.Log($"削除: {objName}");
//                 }
//             }

//             Debug.Log($"卓球ゲームのオブジェクトを削除しました。（{deletedCount}個）");
//             Debug.Log("Setup Game Sceneを実行して再構築してください。");
//         }

//         private static void CreateGameObjects()
//         {
//             // GameManager
//             GameObject gameManager = new GameObject("GameManager");
//             GameManager gmScript = gameManager.AddComponent<GameManager>();
//             GameStarter gameStarter = gameManager.AddComponent<GameStarter>();

//             // 卓球台
//             GameObject table = CreatePingPongTable();

//             // ボール
//             GameObject ball = CreateBall();
//             BallController ballController = ball.AddComponent<BallController>();

//             // プレイヤーラケット（Z軸固定位置）
//             GameObject playerRacket = CreateRacket("PlayerRacket", new Vector3(0, 1.2f, -4));
//             RacketController racketController = playerRacket.AddComponent<RacketController>();

//             // RacketControllerのfixedZPositionを設定
//             SerializedObject serializedRacket = new SerializedObject(racketController);
//             serializedRacket.FindProperty("fixedZPosition").floatValue = -4f;
//             serializedRacket.ApplyModifiedProperties();

//             // AI対戦相手（Z軸固定位置）
//             GameObject aiOpponent = new GameObject("AIOpponent");
//             aiOpponent.transform.position = new Vector3(0, 0, 4);
//             AIOpponent aiScript = aiOpponent.AddComponent<AIOpponent>();

//             // AIOpponentのfixedZPositionを設定
//             SerializedObject serializedAI = new SerializedObject(aiScript);
//             serializedAI.FindProperty("fixedZPosition").floatValue = 4f;
//             serializedAI.ApplyModifiedProperties();

//             // AIラケット
//             GameObject aiRacket = CreateRacket("AIRacket", new Vector3(0, 1.2f, 4));
//             aiRacket.transform.SetParent(aiOpponent.transform);

//             // AIのracketTransformを設定
//             serializedAI.FindProperty("racketTransform").objectReferenceValue = aiRacket.transform;
//             serializedAI.ApplyModifiedProperties();

//             // HitPointManager
//             GameObject hpManager = new GameObject("HitPointManager");
//             HitPointManager hpScript = hpManager.AddComponent<HitPointManager>();

//             // UIManager
//             GameObject uiManager = new GameObject("UIManager");
//             UIManager uiScript = uiManager.AddComponent<UIManager>();

//             // TimingGaugeUI
//             GameObject timingGaugeObj = new GameObject("TimingGaugeUI");
//             TimingGaugeUI timingGaugeUI = timingGaugeObj.AddComponent<TimingGaugeUI>();

//             // FeedbackUI
//             GameObject feedbackObj = new GameObject("FeedbackUI");
//             FeedbackUI feedbackUI = feedbackObj.AddComponent<FeedbackUI>();

//             // GameManagerに参照を設定
//             SerializedObject serializedGM = new SerializedObject(gmScript);
//             serializedGM.FindProperty("ballController").objectReferenceValue = ballController;
//             serializedGM.FindProperty("playerRacket").objectReferenceValue = racketController;
//             serializedGM.FindProperty("aiOpponent").objectReferenceValue = aiScript;
//             serializedGM.FindProperty("hitPointManager").objectReferenceValue = hpScript;
//             serializedGM.FindProperty("uiManager").objectReferenceValue = uiScript;
//             serializedGM.FindProperty("tableTransform").objectReferenceValue = table.transform;
//             serializedGM.ApplyModifiedProperties();

//             // カメラを縦長視点に設定
//             SetupCamera();

//             Debug.Log("卓球ゲームの基本セットアップが完了しました！");
//             Debug.Log("カメラを1920x1080縦長レイアウトに設定しました。");

//             // 選択
//             Selection.activeGameObject = gameManager;
//         }

//         private static GameObject CreatePingPongTable()
//         {
//             GameObject table = new GameObject("PingPongTable");

//             // テーブル本体を縦長（Z軸方向を長く）に変更
//             // 1920x1080画面に対して大きく表示
//             // 縦：横 = 1920:1080 = 1.778:1 の比率で縦長に
//             float tableWidth = 3.0f;  // X軸（横幅）
//             float tableLength = 7.5f; // Z軸（縦長）

//             GameObject tableSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             tableSurface.name = "TableSurface";
//             tableSurface.transform.SetParent(table.transform);
//             tableSurface.transform.localPosition = new Vector3(0, 0.76f, 0);
//             tableSurface.transform.localScale = new Vector3(tableWidth, 0.05f, tableLength);

//             // マテリアル設定
//             Material tableMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             tableMaterial.color = new Color(0.2f, 0.4f, 0.8f); // 青色
//             tableSurface.GetComponent<Renderer>().material = tableMaterial;

//             // 中央線（横向き - X軸方向）
//             GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             centerLine.name = "CenterLine";
//             centerLine.transform.SetParent(table.transform);
//             centerLine.transform.localPosition = new Vector3(0, 0.765f, 0);
//             centerLine.transform.localScale = new Vector3(tableWidth, 0.01f, 0.05f);

//             Material lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             lineMaterial.color = Color.white;
//             centerLine.GetComponent<Renderer>().material = lineMaterial;

//             // ネット（横向き - X軸方向）
//             GameObject net = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             net.name = "Net";
//             net.transform.SetParent(table.transform);
//             net.transform.localPosition = new Vector3(0, 1.0f, 0);
//             net.transform.localScale = new Vector3(tableWidth, 0.2f, 0.05f);

//             Material netMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             netMaterial.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
//             net.GetComponent<Renderer>().material = netMaterial;

//             return table;
//         }

//         private static GameObject CreateBall()
//         {
//             GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//             ball.name = "Ball";
//             ball.transform.position = new Vector3(0, 1.5f, 0);
//             ball.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f); // 40mm

//             // コライダーを削除（物理演算は使わない）
//             Object.DestroyImmediate(ball.GetComponent<SphereCollider>());

//             Material ballMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             ballMaterial.color = Color.white;
//             ball.GetComponent<Renderer>().material = ballMaterial;

//             return ball;
//         }

//         private static GameObject CreateRacket(string name, Vector3 position)
//         {
//             GameObject racket = new GameObject(name);
//             racket.transform.position = position;

//             // ラケット面
//             GameObject racketFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
//             racketFace.name = "RacketFace";
//             racketFace.transform.SetParent(racket.transform);
//             racketFace.transform.localPosition = Vector3.zero;
//             racketFace.transform.localScale = new Vector3(0.15f, 0.15f, 0.01f);
//             racketFace.transform.localRotation = Quaternion.Euler(90, 0, 0);

//             Material racketMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             racketMaterial.color = Color.red;
//             racketFace.GetComponent<Renderer>().material = racketMaterial;

//             // コライダーを削除
//             Object.DestroyImmediate(racketFace.GetComponent<BoxCollider>());

//             // 持ち手
//             GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//             handle.name = "Handle";
//             handle.transform.SetParent(racket.transform);
//             handle.transform.localPosition = new Vector3(0, -0.1f, 0);
//             handle.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);

//             Material handleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
//             handleMaterial.color = new Color(0.6f, 0.3f, 0.1f);
//             handle.GetComponent<Renderer>().material = handleMaterial;

//             // コライダーを削除
//             Object.DestroyImmediate(handle.GetComponent<CapsuleCollider>());

//             return racket;
//         }

//         private static void SetupCamera()
//         {
//             // メインカメラを探す
//             Camera mainCamera = Camera.main;
//             if (mainCamera == null)
//             {
//                 GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
//                 foreach (GameObject obj in allObjects)
//                 {
//                     Camera cam = obj.GetComponent<Camera>();
//                     if (cam != null)
//                     {
//                         mainCamera = cam;
//                         break;
//                     }
//                 }
//             }

//             if (mainCamera != null)
//             {
//                 // 縦長視点に設定（真上から見下ろす形）
//                 mainCamera.transform.position = new Vector3(0, 10, 0);
//                 mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
//                 mainCamera.fieldOfView = 60;
//                 Debug.Log("カメラを縦長視点（真上から）に設定しました。");
//             }
//             else
//             {
//                 Debug.LogWarning("メインカメラが見つかりません。");
//             }
//         }

//         [MenuItem("PingPong/Add Character Model to AI")]
//         public static void AddCharacterModelToAI()
//         {
//             AIOpponent aiOpponent = FindObjectOfType<AIOpponent>();
//             if (aiOpponent == null)
//             {
//                 EditorUtility.DisplayDialog("エラー", "AIOpponentが見つかりません。先にSetup Game Sceneを実行してください。", "OK");
//                 return;
//             }

//             EditorUtility.DisplayDialog("キャラクターモデル追加",
//                 "model_Material/FBX フォルダからHumanoidモデルをAIOpponentオブジェクトの子として手動で配置してください。\n" +
//                 "その後、AIOpponentコンポーネントのCharacter Profilesに設定してください。",
//                 "OK");

//             Selection.activeGameObject = aiOpponent.gameObject;
//         }
//     }
// }
