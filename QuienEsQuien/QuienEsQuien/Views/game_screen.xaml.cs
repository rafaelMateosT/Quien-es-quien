﻿using Manejadoras;
using Microsoft.AspNet.SignalR.Client;
using QuienEsQuien.Modelos;
using QuienEsQuien.Viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuienEsQuien.Views {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 



    public sealed partial class game_screen : Page {


        ChatMessage send = new ChatMessage();
        viewModel vm = new viewModel();
        clsManejadora maneja = new clsManejadora();
        public static HubConnection conn { get; set; }
        public IHubProxy ChatProxy { get; set; }
        string sala;
        App myApp = (Application.Current as App);
        private CoreDispatcher _dispatcher;


        public game_screen() {

            this.InitializeComponent();


            Windows.UI.Core.Preview.SystemNavigationManagerPreview.GetForCurrentView().CloseRequested +=
               async (sender, args) => {
                   args.Handled = true;

               };


            _dispatcher = Window.Current.Dispatcher;
            vm = (viewModel)this.DataContext;
            SignalR();

            cargando();

            send.nickName = vm.nickJugador;
            send.groupName = sala;

            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {

                ChatProxy.Invoke("JoinGroup", myApp.sala);
            }



        }


        private void SignalR() {


            //Connect to the url 
            conn = new HubConnection("https://adivinaquiensoy.azurewebsites.net/");
            //conn = new HubConnection("http://localhost:50268/");
            //ChatHub is the hub name defined in the host program. 

            ChatProxy = conn.CreateHubProxy("ChatHub");
            conn.Start();

            ChatProxy.On<ChatMessage>("agregarMensaje", addMessage);
            ChatProxy.On("abandoPartida", volverLobby);
            ChatProxy.On("cambiarTurno", cambiarTurno);
            ChatProxy.On<clsCarta, string>("comprobarGanador", comprobarGanador);
            ChatProxy.On<string>("finalizarPartidaPorGanador", finalizarPartidaPorGanador);
            ChatProxy.On("falloAdivinar", actualizarIntentos);
            ChatProxy.On<string>("finalizarPartidaPorFallos", finalizarPartidaPorFallos);
            ChatProxy.On("salirAbriptamente", salirPorAbandono);

            //ChatProxy.On<ChatMessage>("agregarMensaje", addMessage);

        }

        private async void salirPorAbandono() {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                    SalirAbruptuamenteRelative.Visibility = Visibility.Visible;

                    int i = 0;
                    do {
                        Thread.Sleep(1000);
                        i++;
                    } while (i < 1);
                });


            });


        }


        private async void finalizarPartidaPorGanador(string obj) {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    ConfirmarGanador.Visibility = Visibility.Visible;
                });

                int i = 0;
                do {
                    Thread.Sleep(1000);
                    i++;
                } while (i < 1);

            });

        }

        private async void comprobarGanador(clsCarta obj, string nickname) {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                if (vm.personageGanador.nombreCarta.Equals(obj.nombreCarta)) {

                    if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {

                        await ChatProxy.Invoke("Ganador", nickname, myApp.sala);
                    }
                } else {

                    if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                        await ChatProxy.Invoke("Perdedor", myApp.sala);
                    }

                }
            });
        }

        private async void actualizarIntentos() {


            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                vm.intentos++;

                switch (vm.intentos) {

                    case 1:
                        primerIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;


                    case 2:
                        primerIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        segundoIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                    case 3:

                        primerIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        segundoIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        tercerIntento.Fill = new SolidColorBrush(Windows.UI.Colors.Red);
                        break;

                }
            });

            if (vm.intentos == 3) {

                if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                    await ChatProxy.Invoke("GanadorPorFallos", myApp.sala, myApp.nickJugador);
                }
            } else {
                if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                    await ChatProxy.Invoke("pasarTurno", myApp.sala);
                }
            }

        }

        private async void finalizarPartidaPorFallos(string nickNamePerdedor) {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                string textoInfo;

                if (nickNamePerdedor == myApp.nickJugador) {
                    textoInfo = "Perdiste, has fallado tres veces";
                    ActualizarUIPorFallos(textoInfo);
                } else {
                    textoInfo = $"Ganaste porque {nickNamePerdedor} falló tres veces.";
                    ActualizarUIPorFallos(textoInfo);

                }
            });
        }

        private async void ActualizarUIPorFallos(string textoInfo) {

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                TextoGandorOPerdedor.Text = textoInfo;
                ConfirmarGanadorPorFallos.Visibility = Visibility.Visible;
            });

            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 1);


        }

        private async void cambiarTurno() {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                if (myApp.miTurno) {
                    myApp.miTurno = false;
                    turno.Text = "No es mi turno";
                } else {
                    myApp.miTurno = true;
                    turno.Text = "Es mi turno";
                }
            });
        }


        public async void ActualizarUi() {

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                ConfirmarGanador.Visibility = Visibility.Collapsed;
            });

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                ConfirmarGanadorPorFallos.Visibility = Visibility.Collapsed;
            });

            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 1);

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Cargando.Visibility = Visibility.Visible;
            });
        }


        private async void volverLobby() {

            ActualizarUi();

            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 3);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                myApp.esVolver = true;
                this.Frame.Navigate(typeof(lobby_screen));

            });
        }

        private async void cargando() {

            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 10 || !(conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected));

            if (myApp.miTurno) {
                turno.Text = "Es tu turno";
            } else {
                turno.Text = "NO ES TU TURNO";
            }

            if (i == 10 && !(conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)) {
                ContentDialog noFunca = new ContentDialog();
                noFunca.Title = "¡Ups!";
                noFunca.Content = "Algo ha salido mal :'(";
                noFunca.PrimaryButtonText = "Salir";

                ContentDialogResult resultado = await noFunca.ShowAsync();

                if (resultado == ContentDialogResult.Primary) {
                    this.Frame.Navigate(typeof(login_screen));
                }
            }
        }

        private async void addMessage(ChatMessage obj) {

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                vm.AñadirAChat(obj);
                contanierChat.ScrollIntoView(contanierChat.Items[contanierChat.Items.Count - 1]);

                contanierChat.SelectedItem = contanierChat.Items.Count - 1;
            });

        }

        private void Btn_Salir_Click(object sender, RoutedEventArgs e) {
            vm.visibilidad = "Visible";
            ChatProxy.Invoke("LeaveGroup", myApp.sala);

        }

        public void SalirPaSiempre() {

            ChatProxy.Invoke("LeaveGroup", myApp.sala);
        }

        private void Btn_Pasar_Click(object sender, RoutedEventArgs e) {

            if (myApp.miTurno) {
                if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                    ChatProxy.Invoke("pasarTurno", myApp.sala);
                }
            }

        }

        public void MostrarSalir() {

                Cargando.Visibility = Visibility.Visible;

                int i = 0;
                do {
                    Thread.Sleep(1000);
                    i++;
                } while (i < 2);
          

           



        }

        public void salirAbruptamente() {



            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 2);


            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                ChatProxy.Invoke("SalirAbruptamente", myApp.sala);
            }

        }

        private void Btn_send_Click(object sender, RoutedEventArgs e) {

            send.message = " " + tbx_chat.Text;
            send.groupName = myApp.sala;
            send.nickName = vm.nickJugador + ": ";

            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {

                ChatProxy.Invoke("SendToGroup", send);
            }

            tbx_chat.Text = "";
        }

        private void Tbx_chat_KeyDown(object sender, KeyRoutedEventArgs e) {

            if (e.Key == Windows.System.VirtualKey.Enter) {

                send.message = " " + tbx_chat.Text;
                send.groupName = myApp.sala;
                send.nickName = vm.nickJugador + ": ";

                if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                    ChatProxy.Invoke("SendToGroup", send);
                }
                tbx_chat.Text = "";
            }
        }

        private void ConfirmarSeleccion_Click(object sender, RoutedEventArgs e) {

            if (conn.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected) {
                ChatProxy.Invoke("sendPosibleWinner", vm.cartaGanadoraSeleccionada, myApp.sala, myApp.nickJugador);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            ConfirmarGanador.Visibility = Visibility.Collapsed;

            ChatProxy.Invoke("LeaveGroup", myApp.sala);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {

            ConfirmarGanadorPorFallos.Visibility = Visibility.Collapsed;

            ChatProxy.Invoke("LeaveGroup", myApp.sala);

        }

        private void CartaSelectPanel_PointerPressed(object sender, PointerRoutedEventArgs e) {
            Storyboard myStory = new Storyboard();

            RelativePanel clickedElement = sender as RelativePanel;

            Object value = null;
            clickedElement?.Resources.TryGetValue("volteaImagen", out value);

            myStory = value as Storyboard;
            //myStory = (Storyboard) gridImagenes.FindName("volteaImagen");
            myStory?.Begin();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {


            SalirAbruptuamenteRelative.Visibility = Visibility.Collapsed;
            Cargando.Visibility = Visibility.Visible;

            int i = 0;
            do {
                Thread.Sleep(1000);
                i++;
            } while (i < 2);

            this.Frame.Navigate(typeof(lobby_screen));

        }
    }
}
