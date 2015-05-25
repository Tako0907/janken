using DxLibDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janken
{
    public class GameData
    {
        #region

        /**************************************
         * ゲーム定数の定義
         **************************************/

        public enum GameProgressStatus
        {
            StartScreen,        /* スタート画面 */
            GamePlay,           /* ゲームプレイ画面 */
            END                 /* ゲーム終了 */
        }

        public enum GamePlayStatus
        {
            Prog01,             /* 手の選択画面まで */
            HandSelect,         /* 手の選択画面 */
            Prog02,             /* 勝敗判定まで */
            SelectContinue,     /* 続けるかの選択画面 */
        }

        enum Hand
        {
            Goo,        /* グー */
            Scissors,   /* チョキ */
            Per         /* パー */
        }

        enum Face
        {
            Win_img,    /* 勝利時の顔 */
            Lose_img,   /* 敗北時の顔 */
            Draw_img    /* 引き分け時の顔 */
        }

        enum JudgeResult
        {
            WIN,
            DRAW,
            LOSE
        }

        #endregion



        #region

        /**************************************
         * プロパティ
         **************************************/

        public GameProgressStatus gmprogstat { get; set; }      /* ゲームの進行状況プロパティ用 */
        public GamePlayStatus gmplaystat { get; set; }          /* ゲームプレイの進行状況プロパティ用 */
        public int GameFPS { get; set; }                        /* ゲームのFPS */

        #endregion

        private int selectMenuId = 0;               /* 選択されたメニュー用変数 */
        private bool isPressKey = false;            /* 前のフレームでキーボードを押していたか */

        private int[] gamePlay_HandImg = new int[] { -1, -1, -1 };    /* 手の画像を格納する配列変数（定数でアクセス可能） */
        private int[] gamePlay_FaceImg = new int[] { -1, -1, -1 };    /* ゲーム結果に出る顔の画像を格納するもの */
        private int frameCounter = 1;                                 /* 何フレーム目かをカウントする変数 */
        private Hand playerHand = 0;                                  /* プレイヤーの選択した手を保存する変数 */
        private Hand enemyHand = 0;                                   /* 敵の選択した手を保存する変数 */
        private JudgeResult judgeResult;                              /* じゃんけんの結果 */

        private int playerWinCount = 0;     /* プレイヤーの勝利数 */
        private int playerLoseCount = 0;    /* プレイヤーの敗北数 */
        private int playerDrawCount = 0;    /* プレイヤーの引き分け数 */
        private int winningStreak = 0;      /* 連勝数 */
        private int losingStreak = 0;       /* 連敗数 */



        #region

        /**************************************
         * 場面の処理メソッド
         **************************************/

        /// <summary>
        /// ゲームのスタート画面の処理メソッド
        /// </summary>
        /// <param name="key">押されたキーボードの情報配列</param>
        /// <returns>正常終了時: 0。それ以外: -1</returns>
        public int Start_Screen(byte[] key)
        {
            // 上キー or 下キーが押されていたらメニュー選択の切り替え
            if ((key[DX.KEY_INPUT_UP] == 1 && !isPressKey) || (key[DX.KEY_INPUT_DOWN] == 1 && !isPressKey))
            {
                selectMenuId = selectMenuId == 0 ? 1 : 0;
                isPressKey = true;
            }

            if (key[DX.KEY_INPUT_RETURN] == 1)      // メニューを選択（Enter）
            {
                // ゲームスタートを選択, ゲーム画面へ
                if (selectMenuId == 0)
                {
                    gmprogstat = GameProgressStatus.GamePlay;
                    InitGamePlay();     // ゲームプレイ画面のリソース準備
                    EndStartScreen();   // スタート画面のリソース開放
                }

                // 終了を選択, 終了画面へ
                else
                {
                    gmprogstat = GameProgressStatus.END;
                    EndStartScreen();   // スタート画面のリソース開放
                }
            }

            // キーフラグの解除
            if (key[DX.KEY_INPUT_UP] == 0 && key[DX.KEY_INPUT_DOWN] == 0 && isPressKey)
            {
                isPressKey = false;
            }

            int x = CalcCenterX("-> ゲームスタート") - DX.GetFontSize(), y = 360;                       // 文字の表示位置
            int selectColor = DX.GetColor(255, 100, 100), menuColor = DX.GetColor(255, 255, 255);       // メニューの文字カラー

            /* タイトル */
            DX.SetFontSize(36);
            DX.DrawString(CalcCenterX("☆じゃんけんゲーム☆"), 235, "☆じゃんけんゲーム☆", DX.GetColor(255, 153, 0));

            DX.SetFontSize(16);


            // 選択されているメニューによって表示を変える
            switch (selectMenuId)
            {
                case 0:     // ゲームスタートが選択されている
                    DX.DrawString(x, y, "-> ゲームスタート", selectColor);
                    DX.DrawString(x, (int)(y + DX.GetFontSize() * 1.5), "   終了", menuColor);
                    break;
                case 1:     // 終了が選択されている
                    DX.DrawString(x, y, "   ゲームスタート", menuColor);
                    DX.DrawString(x, (int)(y + DX.GetFontSize() * 1.5), "-> 終了", selectColor);
                    break;
            }

            return 0;
        }

        /// <summary>
        /// ゲームプレイ画面の処理メソッド
        /// </summary>
        /// <param name="key">押されたキーボードの情報配列</param>
        /// <returns>正常終了時: 0。それ以外: -1</returns>
        public int GamePlay(byte[] key)
        {
            switch (gmplaystat)
            {
                case GamePlayStatus.Prog01: // ゲームプレイ第1場面
                    if (frameCounter <= 60) // 60フレーム以下（2秒間）
                    {

                        DX.SetFontSize(24); // 文字のサイズの変更
                        DX.DrawString(CalcCenterX("ゲームを開始します"), 288, "ゲームを開始します", DX.GetColor(200, 200, 200));

                        DX.SetFontSize(16);
                        frameCounter++;
                    }

                    else if (frameCounter <= 120) // 120フレーム以下（2秒間）
                    {
                        DX.SetFontSize(24);
                        DX.DrawString(CalcCenterX("最初はグー！"), 288, "最初はグー！", DX.GetColor(200, 200, 200));
                        DX.DrawRotaGraph(400, 100, 0.34, Math.PI, gamePlay_HandImg[(int)Hand.Goo], DX.TRUE);   // 相手の手の画像を表示

                        DX.SetFontSize(16);
                        frameCounter++;

                    }
                    else if (frameCounter <= 180) // 180フレーム以下（2秒間）
                    {
                        DX.SetFontSize(24);
                        DX.DrawString(CalcCenterX("じゃんけん！"), 288, "じゃんけん！", DX.GetColor(200, 200, 200));
                        DX.DrawRotaGraph(400, 100, 0.34, Math.PI, gamePlay_HandImg[(int)Hand.Goo], DX.TRUE);   // 相手の手の画像を表示

                        DX.SetFontSize(16);
                        frameCounter++;
                    }
                    else // 次の処理へ
                    {
                        frameCounter = 1;
                        gmplaystat = GamePlayStatus.HandSelect;     // 次の場面へ行くためのフラグセット
                    }
                    break;

                case GamePlayStatus.HandSelect: // 手を選択する場面
                    DX.SetFontSize(24);
                    DX.DrawString(CalcCenterX("手を選んでください！"), 264, "手を選んでください！", DX.GetColor(200, 200, 200));
                    DX.DrawString(CalcCenterX("グー…0　チョキ…1　パー…2"), 288, "グー…0　チョキ…1　パー…2", DX.GetColor(255, 100, 100));

                    if (winningStreak > 1)
                        DX.DrawString(700, 0, winningStreak + "連勝", DX.GetColor(255, 51, 0));   //連勝の表示
                    if (losingStreak > 1)
                        DX.DrawString(700, 0, losingStreak + "連敗", DX.GetColor(0, 153, 255));   //連敗の表示


                    DX.DrawRotaGraph(400, 100, 0.34, Math.PI, gamePlay_HandImg[(int)Hand.Goo], DX.TRUE);   // 相手の手の画像を表示

                    DX.DrawRotaGraph(100, 500, 0.34, 0, gamePlay_HandImg[(int)Hand.Goo], DX.TRUE);        // 手の画像を表示　グー
                    DX.DrawRotaGraph(400, 495, 0.34, 0, gamePlay_HandImg[(int)Hand.Scissors], DX.TRUE);   // 手の画像を表示　チョキ
                    DX.DrawRotaGraph(700, 500, 0.34, 0, gamePlay_HandImg[(int)Hand.Per], DX.TRUE);        // 手の画像を表示　パー


                    if (key[DX.KEY_INPUT_0] == 1)   // グーを選択
                    {
                        playerHand = 0; // 自分の手を格納
                        enemyHand = (Hand)DX.GetRand(2);      // 敵の手を決定
                        gmplaystat = GamePlayStatus.Prog02;  // 次の場面へ行くためのフラグセット
                    }
                    else if (key[DX.KEY_INPUT_1] == 1)  // チョキを選択
                    {
                        playerHand = (Hand)1;
                        enemyHand = (Hand)DX.GetRand(2);      // 敵の手を決定
                        gmplaystat = GamePlayStatus.Prog02;
                    }
                    else if (key[DX.KEY_INPUT_2] == 1)  // パーを選択
                    {
                        playerHand = (Hand)2;
                        enemyHand = (Hand)DX.GetRand(2);      // 敵の手を決定
                        gmplaystat = GamePlayStatus.Prog02;
                    }

                    DX.SetFontSize(16);
                    break;

                case GamePlayStatus.Prog02: // 勝敗判定
                    if (frameCounter <= 60)    // 60フレーム以下（2秒間）
                    {
                        DX.SetFontSize(24);
                        DX.DrawString(CalcCenterX("ポン！"), 288, "ポン！", DX.GetColor(200, 200, 200));
                        DX.DrawRotaGraph(400, 100, 0.34, Math.PI, gamePlay_HandImg[(int)enemyHand], DX.TRUE);    // 相手の手の画像を表示
                        DX.DrawRotaGraph(400, 495, 0.34, 0, gamePlay_HandImg[(int)playerHand], DX.TRUE);         // 自分の手の画像を表示
                        judgeResult = Judge(playerHand, enemyHand);     // 判定
                        frameCounter++;
                    }
                    else if (frameCounter <= 150)    // 150フレーム以下（3秒間）
                    {
                        DX.SetFontSize(24);

                        switch (judgeResult)     // 判定ごとの表示
                        {
                            case JudgeResult.WIN:
                                DX.DrawRotaGraph(390, 150, 0.34, 0, gamePlay_FaceImg[(int)Face.Win_img], DX.TRUE);
                                DX.DrawString(CalcCenterX("あなたの勝ち！！"), 288, "あなたの勝ち！！", DX.GetColor(255, 50, 50));

                                if (frameCounter <= 61) // プレイヤーの勝利数の増加（1フレーム）
                                {
                                    playerWinCount++;   // プレイヤーの勝利数の加算
                                    winningStreak++;    // 連勝数の加算
                                }

                                losingStreak = 0;       // 連敗カウントのリセット
                                break;

                            case JudgeResult.DRAW:
                                DX.DrawRotaGraph(390, 150, 0.34, 0, gamePlay_FaceImg[(int)Face.Draw_img], DX.TRUE);
                                DX.DrawString(CalcCenterX("引き分け"), 288, "引き分け", DX.GetColor(50, 255, 50));

                                if (frameCounter <= 61) // プレイヤー引き分け数の増加（1フレーム）
                                    playerDrawCount++;

                                winningStreak = 0;      // 連勝カウントのリセット
                                losingStreak = 0;       // 連敗カウントのリセット
                                break;

                            case JudgeResult.LOSE:
                                DX.DrawRotaGraph(390, 150, 0.34, 0, gamePlay_FaceImg[(int)Face.Lose_img], DX.TRUE);
                                DX.DrawString(CalcCenterX("あなたの負け・・"), 288, "あなたの負け・・", DX.GetColor(50, 50, 255));

                                if (frameCounter <= 61) // プレイヤーの敗北数の増加（1フレーム）
                                {
                                    playerLoseCount++;  // プレイヤーの敗北数の加算
                                    losingStreak++;     // 連敗数の加算
                                }

                                winningStreak = 0;      // 連勝カウントのリセット
                                break;
                        }

                        frameCounter++;
                    }
                    else    // 終了判定
                    {
                        // 上キー or 下キーが押されていたらメニュー選択の切り替え
                        if ((key[DX.KEY_INPUT_UP] == 1 && !isPressKey) || (key[DX.KEY_INPUT_DOWN] == 1 && !isPressKey))
                        {
                            selectMenuId = selectMenuId == 0 ? 1 : 0;
                            isPressKey = true;
                        }

                        if (key[DX.KEY_INPUT_RETURN] == 1)      // メニューを選択（Enter）
                        {
                            // もう一度を選択, ゲーム画面へ
                            if (selectMenuId == 0)
                            {
                                gmplaystat = GamePlayStatus.Prog01;
                                frameCounter = 1;
                            }

                            // 終了を選択, 終了画面へ
                            else
                            {
                                gmprogstat = GameProgressStatus.END;
                                EndGamePlay();   // ゲーム画面のリソース開放
                            }
                        }

                        // キーフラグの解除
                        if (key[DX.KEY_INPUT_UP] == 0 && key[DX.KEY_INPUT_DOWN] == 0 && isPressKey)
                        {
                            isPressKey = false;
                        }

                        int x = CalcCenterX("もう一度") - DX.GetFontSize(), y = 360;                                // 文字の表示位置
                        int selectColor = DX.GetColor(255, 100, 100), menuColor = DX.GetColor(255, 255, 255);       // メニューの文字カラー

                        String score = string.Format("{0:00}勝 {1:00}敗 {2:00}分", playerWinCount, playerLoseCount, playerDrawCount);
                        DX.DrawString(CalcCenterX(score), 235, score, DX.GetColor(255, 255, 255));   //勝敗の表示

                        // 選択されているメニューによって表示を変える
                        switch (selectMenuId)
                        {
                            case 0:     // もう一度が選択されている
                                DX.DrawString(x, y, "もう一度", selectColor);
                                DX.DrawString(x, (int)(y + DX.GetFontSize() * 1.5), "終了", menuColor);
                                break;
                            case 1:     // 終了が選択されている
                                DX.DrawString(x, y, "もう一度", menuColor);
                                DX.DrawString(x, (int)(y + DX.GetFontSize() * 1.5), "終了", selectColor);
                                break;
                        }
                    }
                    DX.SetFontSize(16);
                    break;
            }

            return 0;
        }

        #endregion



        #region

        /**************************************
         * 場面ごとの初期化・終了時のメソッド
         **************************************/

        /// <summary>
        /// スタート画面で使用する画像などの初期化をするメソッド
        /// </summary>
        public void InitStartScreen()
        {

        }

        /// <summary>
        /// スタート画面ではなくなる時に呼び出すメソッド
        /// </summary>
        public void EndStartScreen()
        {

        }

        /// <summary>
        /// ゲームプレイ画面で使用する画像などの初期化をするメソッド
        /// </summary>
        public void InitGamePlay()
        {
            gamePlay_HandImg[(int)Hand.Goo] = DX.LoadGraph("img/gamePlay_HandGoo.png");                     // グーの画像の読み込み
            gamePlay_HandImg[(int)Hand.Scissors] = DX.LoadGraph("img/gamePlay_HandScissors.png");           // チョキの画像の読み込み
            gamePlay_HandImg[(int)Hand.Per] = DX.LoadGraph("img/gamePlay_HandPer.png");                     // パーの画像の読み込み

            gamePlay_FaceImg[(int)Face.Win_img] = DX.LoadGraph("img/result_win.png");                       // ゲームの結果表示の画像（勝ち）
            gamePlay_FaceImg[(int)Face.Lose_img] = DX.LoadGraph("img/result_lose.png");                     // ゲームの結果表示の画像（負け）
            gamePlay_FaceImg[(int)Face.Draw_img] = DX.LoadGraph("img/result_draw.png");                     // ゲームの結果表示の画像（引き分け）
        }

        /// <summary>
        /// ゲームプレイ画面ではなくなる時に呼び出すメソッド
        /// </summary>
        public void EndGamePlay()
        {
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Goo]);           // リソースの解放
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Scissors]);      // リソースの解放
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Per]);           // リソースの解放

            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Win_img]);       // リソースの解放
            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Lose_img]);      // リソースの解放
            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Draw_img]);      // リソースの解放
        }

        /// <summary>
        /// デストラクター
        /// </summary>
        ~GameData()
        {
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Goo]);           // リソースの解放
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Scissors]);      // リソースの解放
            DX.DeleteGraph(gamePlay_HandImg[(int)Hand.Per]);           // リソースの解放

            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Win_img]);       // リソースの解放
            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Lose_img]);      // リソースの解放
            DX.DeleteGraph(gamePlay_FaceImg[(int)Face.Draw_img]);      // リソースの解放
        }

        #endregion



        /// <summary>
        /// 文字列の中央表示をするために必要なX座標を取得するメソッド
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns></returns>
        private int CalcCenterX(string str)
        {
            int x1 = 0, x2 = 0;
            int y = 0;
            int strWidth;

            DX.GetWindowSize(out x2, out y);
            strWidth = DX.GetDrawStringWidth(str, Encoding.GetEncoding("Shift_JIS").GetByteCount(str));

            return (int)((x1 + ((x2 - x1) / 2)) - (strWidth / 2));
        }

        /// <summary>
        /// じゃんけんの判定をするメソッド
        /// </summary>
        /// <param name="playerH">プレイヤーの手</param>
        /// <param name="enemyH">相手の手</param>
        /// <returns></returns>
        private JudgeResult Judge(Hand playerH, Hand enemyH)
        {
            int j = ((int)playerHand - (int)enemyH + 3) % 3;

            switch (j)
            {
                case 0:
                    return JudgeResult.DRAW;
                case 2:
                    return JudgeResult.WIN;
                default:
                    return JudgeResult.LOSE;
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public GameData()
        {
            gmprogstat = GameProgressStatus.StartScreen;
            gmplaystat = GamePlayStatus.Prog01;
            InitStartScreen();      // スタート画面のリソース準備
        }
    }
}