using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels
{
    public class CreditStandingChangedMessage
    {
        public int CustomerId { get; set; }
        public decimal Payment { get; set; }
    }
}
