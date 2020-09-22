using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaleApiCore.Models
{
    public class Info
    {
        public int InfoId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }
    public class Setting
    {
        public int SettingId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
