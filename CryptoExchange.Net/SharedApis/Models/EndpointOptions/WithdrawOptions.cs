﻿using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.FilterOptions
{

    public record WithdrawOptions
    {

        public WithdrawOptions()
        {
        }

        public Error? Validate(WithdrawRequest request)
        {

            return null;
        }
    }
}