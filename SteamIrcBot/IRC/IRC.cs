﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeronIRC;
using System.Threading;

namespace SteamIrcBot
{
    class IRC
    {
        static IRC _instance = new IRC();
        public static IRC Instance { get { return _instance; } }


        public CommandManager CommandManager { get; private set; }

        public AutoResetEvent JoinEvent { get; private set; }


        IrcClient client;
        bool shuttingDown = false;

        DateTime nextConnect;


        IRC()
        {
            nextConnect = DateTime.MaxValue;

            JoinEvent = new AutoResetEvent( false );

            client = new IrcClient( Settings.Current.IRCNick );

            CommandManager = new CommandManager( client );

            client.AlternateNickname = Settings.Current.IRCNick + "_";
            client.RealName = Settings.Current.IRCNick;
            client.Ident = "steamircbot";
            client.OutputRateLimit = 800;


            client.ConnectionParser.Connected += OnConnected;
            client.ConnectionParser.Disconnected += OnDisconnected;

            client.ChannelParser.Join += OnJoin;
        }



        public void Connect()
        {
            if ( client.IsConnected )
                return;

            Reconnect( TimeSpan.Zero );
        }

        public void Disconnect()
        {
            shuttingDown = true;

            client.Disconnect( "fork it all" );
        }


        public void Send( string channel, string format, params object[] args )
        {
            if ( !client.IsConnected )
                return;

            client.SendMessage( channel, string.Format( format, args ) );
        }
        public void SendAnnounce( string format, params object[] args )
        {
            Send( Settings.Current.IRCAnnounceChannel, format, args );
        }
        public void SendAll( string format, params object[] args )
        {
            Send( string.Format( "{0},{1}", Settings.Current.IRCMainChannel, Settings.Current.IRCAnnounceChannel ), format, args );
        }


        public void Tick()
        {
            if ( DateTime.Now >= nextConnect )
            {
                // try forcing a disconnect in case heronirc forgot it's state 
                client.Disconnect();

                nextConnect = DateTime.MaxValue;

                Log.WriteInfo( "IRC", "Connecting..." );
                client.Connect( Settings.Current.IRCServer, Settings.Current.IRCPort );
            }

            CommandManager.Tick();
        }

        void Reconnect( TimeSpan when )
        {
            nextConnect = DateTime.Now + when;
        }


        void OnConnected( object sender, InfoEventArgs e )
        {
            Log.WriteInfo( "IRC", "Connected!" );

            client.JoinChannel( string.Format( "{0},{1}", Settings.Current.IRCMainChannel, Settings.Current.IRCAnnounceChannel ) );
        }

        void OnDisconnected( object sender, EventArgs e )
        {
            if ( shuttingDown )
            {
                Log.WriteInfo( "IRC", "Disconnected due to service shutdown" );
                return;
            }

            Log.WriteInfo( "IRC", "Disconnected, reconnecting in 5..." );

            Reconnect( TimeSpan.FromSeconds( 5 ) );
        }

        void OnJoin( object sender, ChannelEventArgs e )
        {
            if ( e.Nickname.Nickname == client.Nickname )
            {
                Log.WriteInfo( "IRC", "Joined channel" );
                JoinEvent.Set();
            }
        }
    }
}
