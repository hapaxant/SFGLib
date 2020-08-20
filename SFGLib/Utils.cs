using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFGLib
{
    public static class Utils
    {
        public static void BresenhamsLine(BlockLineMessage msg, Action<int, int> place) => BresenhamsLine((int)msg.Start.X, (int)msg.Start.Y, (int)msg.End.X, (int)msg.End.Y, place);
        public static void BresenhamsLine(int x1, int y1, int x2, int y2, Action<int, int> place)
        {//ripped straight off https://github.com/SirJosh3917/smiley-face-game/blob/master/packages/api/src/misc.ts :D
            int width = x2 - x1;
            int height = y2 - y1;

            int dirX1 = (width < 0 ? -1 : (width > 0 ? 1 : 0)), dirX2 = dirX1;
            int dirY1 = (height < 0 ? -1 : (height > 0 ? 1 : 0)), dirY2 = dirY1;

            var longest = Math.Abs(width);
            var shortest = Math.Abs(height);
            if (!(longest > shortest))
            {
                var tmp = shortest;
                shortest = longest;
                longest = tmp;

                dirX2 = 0;
            }
            else
            {
                dirY2 = 0;
            }

            var numerator = longest / 2;
            int x = x1, y = y1;
            for (var i = 0; i <= longest; i++)
            {
                place(x, y);
                numerator += shortest;

                if (numerator < longest)
                {
                    x += dirX2;
                    y += dirY2;
                }
                else
                {
                    numerator -= longest;
                    x += dirX1;
                    y += dirY1;
                }
            }
        }
    }
}