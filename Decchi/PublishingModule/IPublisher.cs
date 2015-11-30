using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublishingModule
{
    internal interface IDecchiPublisher
    {
        bool Login();

        bool Publish(string text);
    }
}
