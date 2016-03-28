using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapClient.User_Interfaces.NetworkMap.DX
{
    public class RenderTime
    {
        TimeSpan _elapsedTime;
        TimeSpan _totalTime;

        /// <summary>
        /// Time elapsed since last frame
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get
            {
                return _elapsedTime;
            }
        }

        /// <summary>
        /// Total time since start.
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return _totalTime;
            }
        }

        /// <summary>
        /// Create a new render time tracking object.
        /// </summary>
        public RenderTime()
        {
            _elapsedTime = TimeSpan.Zero;
            _totalTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Accumulate time since last render cycle.
        /// </summary>
        /// <param name="elapsed"></param>
        public void Update(TimeSpan elapsed)
        {
            _elapsedTime = elapsed;
            _totalTime += elapsed;
        }
    }
}
