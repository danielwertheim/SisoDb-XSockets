﻿using System;
using System.ComponentModel.Composition;
using System.Configuration;
using XSockets.Core.Plugin.MEF;
using XSockets.Core.XSocket.Interface;

namespace Server
{
    public class Start : Composable
    {
        private bool _started;
        private readonly bool _policyServerStarted;

        [Import(typeof(IXBaseServerContainer), RequiredCreationPolicy = CreationPolicy.Shared)]
        protected IXBaseServerContainer wss { get; set; }

        [Import(typeof(IPolicyServer))]
        protected IPolicyServer PolicyServer { get; private set; }

        public Start()
        {
            try
            {
                //Try to start policyserver...
                if (bool.Parse(ConfigurationManager.AppSettings["UsePolicyServer"]))
                {
                    var host = ConfigurationManager.AppSettings["XSocketServerLocation"];
                    PolicyServer.Start("crossdomain.xml", host);
                    _policyServerStarted = true;
                }
            }
            catch
            {

            }

            wss.OnServersStarted += wss_OnServersStarted;
            wss.OnServersStopped += wss_OnServersStopped;
            wss.StartServers();
            Menu();
        }

        public void Menu()
        {
            try
            {

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("> ");
                    var cmd = Console.ReadLine().ToLower();

                    if (cmd == "quit")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Terminated servers...");
                        break;
                    }
                    if (cmd == "stop")
                    {
                        wss.StopServers();
                    }
                    else if (cmd == "status")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\\Status");
                        Console.WriteLine("________________________________________________________________________");
                        Console.WriteLine("Total Connections:   " + wss.TotalNumberOfConnections);
                        Console.WriteLine("Current Connections: " + wss.CurrentNumberOfConnections);
                        Console.WriteLine("Messages In:         " + wss.NumberOfInMessages);
                        Console.WriteLine("Messages Out:        " + wss.NumberOfOutMessages);
                        Console.WriteLine("Messages Tot:        " + (wss.NumberOfOutMessages + wss.NumberOfInMessages));
                        Console.WriteLine("Total Nr Of Errors:  " + wss.TotalNumberOfErrors);
                        Console.WriteLine("Server Started:      " + wss.ServerStarted);
                        Console.WriteLine("Server Running for:  " + (DateTime.Now - wss.ServerStarted).TotalMinutes + " minutes");
                        Console.WriteLine("PolicyServer Running:" + _policyServerStarted);
                        Console.WriteLine("________________________________________________________________________");
                    }
                    else if (cmd == "handlers")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\\Handlers");
                        Console.WriteLine("________________________________________________________________________");
                        foreach (var plugin in wss.XSocketPlugins)
                        {
                            Console.WriteLine("Alias:\t\t" + plugin.Value.Alias);
                            Console.WriteLine("BufferSize:\t" + plugin.Value.BufferSize);
                            Console.WriteLine("Custom Events:");
                            if (plugin.Value.CustomEvents == null)
                                Console.WriteLine("\tNone...");
                            else
                                foreach (var customEvent in plugin.Value.CustomEvents)
                                    Console.WriteLine("\tName:\t" + customEvent.Value.MethodInfo.Name);
                            Console.WriteLine("");
                        }
                        Console.WriteLine("________________________________________________________________________");
                    }
                    else if (cmd == "protocols")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\\Protocols");
                        Console.WriteLine("________________________________________________________________________");
                        foreach (var plugin in wss.XSocketProtocolPlugins)
                        {
                            Console.WriteLine("Identifier: " + plugin.Value.ProtocolIdentifier);
                        }
                        Console.WriteLine("________________________________________________________________________");
                    }
                    else if (cmd == "interceptors")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\\Interceptors");
                        Console.WriteLine("________________________________________________________________________");
                        foreach (var plugin in wss.MessageInterceptors)
                        {
                            Console.WriteLine("Type: " + plugin.GetType().Name);
                        }
                        foreach (var plugin in wss.ConnectionInterceptors)
                        {
                            Console.WriteLine("Type: " + plugin.GetType().Name);
                        }
                        foreach (var plugin in wss.HandshakeInterceptors)
                        {
                            Console.WriteLine("Type: " + plugin.GetType().Name);
                        }
                        foreach (var plugin in wss.ErrorInterceptors)
                        {
                            Console.WriteLine("Type: " + plugin.GetType().Name);
                        }
                        Console.WriteLine("________________________________________________________________________");
                    }
                    else if (cmd == "start")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        if (this._started)
                        {
                            Console.WriteLine("Servers already started...");
                        }
                        else
                        {
                            Console.WriteLine("Starting servers...");
                            wss.StartServers();
                        }
                    }
                    else if (cmd == "help")
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\\Help");
                        Console.WriteLine("________________________________________________________________________");
                        Console.WriteLine("stop\t\t" +
                                          "Disconnect all client and stop listening for connections" + Environment.NewLine +
                                          "\t\tAlso removes/unloads all handler plugins"
                                         );
                        Console.WriteLine();
                        Console.WriteLine("start\t\t" +
                                          "Starts accepting connections" + Environment.NewLine +
                                          "\t\tAlso registers all handler plugins"
                                         );
                        Console.WriteLine();
                        Console.WriteLine("quit\t\tTerminate development server");
                        Console.WriteLine();
                        Console.WriteLine("status\t\tDisplay server info");
                        Console.WriteLine();
                        Console.WriteLine("handlers\tDisplay registered plugins");
                        Console.WriteLine();
                        Console.WriteLine("protocols\tDisplay registered protocols");
                        Console.WriteLine();
                        Console.WriteLine("interceptors\tDisplay registered interceptors");
                        Console.WriteLine();
                        Console.WriteLine("help\t\tShow commands");
                        Console.WriteLine("________________________________________________________________________");
                    }
                    else
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[XSockets Development Server]");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown command...");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[XSockets Development Server]");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ohh nooooo...");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("");
            }
        }

        void wss_OnServersStarted(object sender, EventArgs e)
        {
            this._started = true;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[XSockets Development Server]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[Servers Started]");
            Console.WriteLine("Type 'help' for available commands");
        }

        void wss_OnServersStopped(object sender, EventArgs e)
        {
            this._started = false;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[XSockets Development Server]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[Servers Stopped]");
            Console.WriteLine("Type 'start' to restart servers, 'help' for available commands");
        }
    }
}