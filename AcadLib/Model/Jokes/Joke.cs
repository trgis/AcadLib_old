﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadLib.Jokes
{
    /// <summary>
    /// Шутка - запуск ссылки
    /// </summary>
    class JokeUrl : IJoke
    {
        private string url;
        public JokeUrl (string url)
        {
            this.url = url;
        }

        public void Show ()
        {
            System.Diagnostics.Process.Start(url);
        }

        public static List<IJoke> Load ()
        {
            var urls = LoadUrls();
            List<IJoke> res = new List<IJoke>();
            foreach (var item in urls)
            {
                var jokeUrl = new JokeUrl(item);
                res.Add(jokeUrl);
            }
            return res;
        }

        private static List<string> LoadUrls ()
        {
            return new List<string> {
                "https://www.youtube.com/embed/T9RFb8xXZlk?autoplay=1",
            };
        }
    }
}