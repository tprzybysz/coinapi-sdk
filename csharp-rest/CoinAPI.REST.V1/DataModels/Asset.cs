﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.REST.V1 {
  public  class Asset {


        public string asset_id { get; set; }
        public string name { get; set; }
        public bool type_is_crypto { get; set; }


    }
}
