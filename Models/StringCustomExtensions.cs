using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomExtensions
{
    public static class StringCustomExtensions
    {
        public static bool ContainsTags(this string instance, string tagsString)
        {
            if (instance != null)
            {
                string[] tags = tagsString.Split(' ');
                instance = instance.ToLower();
                foreach (string tag in tags)
                {
                    if (instance.Contains(tag.Trim().ToLower()))
                        return true;
                }
            }
            return false;
        }
    }
}