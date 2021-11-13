using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisneyAPI.ViewModel.Error
{
    public class ErrorResVM
    {
        public string ExMessage { get; set; }
        public string CustomMessage { get; set; }
        public override string ToString()
        {
            return ExMessage + " " + CustomMessage;
        }
    }
}
