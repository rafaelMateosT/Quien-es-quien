﻿using Manejadoras;
using Microsoft.AspNet.SignalR.Client;
using QuienEsQuien.Modelos;
using QuienEsQuien.Viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuienEsQuien.Views {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 



    public sealed partial class lobby_screen : Page {

        public static HubConnection conn { get; set; }
        public IHubProxy ChatProxy { get; set; }
        public static IHubProxy SalasProxy { get; set; }

        viewModel miVM = new viewModel();
        string nick = "manu";
        Boolean estaSala = false;
        App myApp = (Application.Current as App);

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            miVM.nickJugador = (string) e.Parameter;
        }

        public lobby_screen() {
            this.InitializeComponent();
            //nickPlayer.Text = nick;
            SignalR();
            miVM = (viewModel)this.DataContext;
            //miVM.nickJugador = nick;
        }


        private void SignalR() {

            //Connect to the url 
            conn = new HubConnection("https://adivinaquiensoy.azurewebsites.net/");
            //conn = new HubConnection("http://localhost:50268/");
            //ChatHub is the hub name defined in the host program. 
            SalasProxy = conn.CreateHubProxy("SalasHub");
            //ChatProxy = conn.CreateHubProxy("ChatHub");
            conn.Start();

            SalasProxy.On<clsSala>("ContarUsuarios", onInfo);
            SalasProxy.On<String>("PasarAJuego", PasarAJugar);
            //ChatProxy.On<ChatMessage>("agregarMensaje", addMessage);

        }


        private async void PasarAJugar(string salaNombre) {


            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                if (!myApp.sala.Equals("") && estaSala == false) {

                    estaSala = true;
                    this.Frame.Navigate(typeof(game_screen));

                }
               

            });

        }


        private async void addMessage(ChatMessage obj) {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

            });
        }

        public static void Position(clsSala info) {

            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {

                SalasProxy.Invoke("JoinRoomAsync", info);

            }
        }


        /*private void addToGroup(string groupName)
        {
            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
            {
                ChatProxy.Invoke("JoinGroup", groupName);
            }
        }
        */

        private async void onInfo(clsSala obj) {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                clsManejadora manejadora = new clsManejadora();
                manejadora.actualizarUsuariosSala(obj);


                var sala = (clsSala)listSalas.Items[obj.id - 1];
                sala.usuariosConectados = obj.usuariosConectados;

             

            });



            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {

                SalasProxy.Invoke("PasarAsalaServer", obj.nombre);

            }


        }

    }
}
