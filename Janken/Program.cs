using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DxLibDLL;


namespace Janken
{
    class Program
    {
        const int GAME_FPS = 30;        // ゲームのFPS

        static void Main(string[] args)
        {
            byte[] key = new byte[256];         // 押されたキーボードの格納用配列

            DX.ChangeWindowMode(DX.TRUE);       // 画面をウィンドウモードにする
            DX.SetGraphMode(800, 600, 32);      // 画面の解像度設定

            /* DXライブラリの初期化 */
            if (DX.DxLib_Init() == -1)
            {
                return;     // エラーの場合は終了
            }

            DX.SetDrawScreen(DX.DX_SCREEN_BACK);        // 描画先を裏画面にする

            GameData gameData = new GameData();     // ゲームクラスの初期化
            FPS fps = new FPS(GAME_FPS);            // FPS制御クラスの初期化

            gameData.GameFPS = GAME_FPS;            // FPSのセット

            /* メインループ */
            while (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0 && DX.GetHitKeyStateAll(out key[0]) == 0 && fps.Update())
            {
                if (gameData.gmprogstat == GameData.GameProgressStatus.END)     // 終了画面へ
                    break;

                switch (gameData.gmprogstat)
                {
                    case GameData.GameProgressStatus.StartScreen:
                        gameData.Start_Screen(key);
                        break;

                    case GameData.GameProgressStatus.GamePlay:
                        gameData.GamePlay(key);
                        break;
                }

                DX.DrawString(0, 0, fps.getFPS().ToString("0.0fps"), DX.GetColor(100, 100, 100));       // FPS表示
                fps.wait();     // 待機
            }

            DX.DxLib_End();     // DXライブラリの終了
        }
    }
}