using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamKit2;

namespace SteamIrcBot
{
    class EUniverseCommand : Command
    {
        public EUniverseCommand()
        {
            Triggers.Add( "!euniverse" );
            HelpText = "!euniverse <euniverse> - Returns the EUniverse string for a given value";
        }

        protected override void OnRun( CommandDetails details )
        {
            if ( details.Args.Length == 0 )
            {
                IRC.Instance.Send( details.Channel, "{0}: EUniverse argument required", details.Sender.Nickname );
                return;
            }

            string inputEUniverse = details.Args[ 0 ];
            int eUniverse;

            if ( !int.TryParse( inputEUniverse, out eUniverse ) )
            {
                IRC.Instance.Send( details.Channel, "{0}: Invalid EUniverse value", details.Sender.Nickname );
                return;
            }

            IRC.Instance.Send( details.Channel, "{0}: {1}", details.Sender.Nickname, ( EUniverse )eUniverse );
        }
    }
}
