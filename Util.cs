using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VirtualEmily
{
    class Util
    {
        static int n; 

        public static void PlayMedia(MediaElement mediaElement, TimeSpan start, TimeSpan length, bool muted)
        {
            mediaElement.Stop();
            mediaElement.Position = start;
            mediaElement.IsMuted = muted;
            Console.WriteLine("Playing video at {0}", start);
            mediaElement.Play();

            DispatcherTimer timer = null;
            n++;
            int x = n;

            timer = new DispatcherTimer(length, DispatcherPriority.Send,
                (_s, _e) => {
                    if (x == n) {
                        mediaElement.Pause();
                        timer.Stop();
                    }
                },
                mediaElement.Dispatcher);
        }



    }
}
