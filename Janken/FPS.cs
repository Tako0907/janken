using DxLibDLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janken
{
    // FPSの制御クラス
    public class FPS
    {
        private int starttime;
        private int count;
        private float mfps;
        private int n = 30;
        private int fps = 30;

        /// <summary>
        /// コンストラクター
        /// 自動的にFPSは30に設定されます
        /// </summary>
        public FPS()
        {
            starttime = 0;
            count = 0;
            mfps = 0;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="setFPS">設定したいFPS</param>
        public FPS(int setFPS)
        {
            starttime = 0;
            count = 0;
            mfps = 0;
            n = fps = setFPS;
        }

        public bool Update()
        {
            if (count == 0)
                starttime = DX.GetNowCount();
            if (count == n)
            {
                int t = DX.GetNowCount();
                mfps = 1000 / ((t - starttime) / (float)n);
                count = 0;
                starttime = t;
            }
            count++;
            return true;
        }

        /// <summary>
        /// 現在のFPSの値を取得するメソッド
        /// </summary>
        /// <returns>現在のFPS</returns>
        public float getFPS()
        {
            return mfps;
        }

        /// <summary>
        /// FPS制御メソッド
        /// </summary>
        public void wait()
        {
            int ttime = DX.GetNowCount() - starttime;
            int wtime = count * 1000 / fps - ttime;
            if (wtime > 0)
                DX.WaitTimer(wtime);
        }
    }
}