﻿using AdivinaQuienSoyService.Manejadora;
using AdivinaQuienSoyService.Models;
using divinaQuienSoyService.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AdivinaQuienSoyService.Hubs
{
    public class ChatHub : Hub {

        clsManejadora maneja = new clsManejadora();
        int id = 0;
        private static int[] UsuariosConectados = new int[10];
        
        private static object lockFlama = new object();
        
        /// <summary>
        /// Añade una conexion a un grupo
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>

        public void JoinGroup(string groupName)
        {

            lock (lockFlama) {

                int id = maneja.ObtenerIDSala(groupName);
                UsuariosConectados[id]++;
                Groups.Add(Context.ConnectionId, groupName);

                

                if (UsuariosConectados[id] == 2) {

                    Clients.Caller.cambiarMiTurno(UsuariosConectados[id]);
                }

            }
              

            

        }

        public void pasarTurno(String groupName) {

            Clients.Group(groupName).cambiarTurno();
        }

        public void MandarInfoJugadoresConectados(string nombresala) {

            Clients.Group(nombresala).actualizarJugadoresConectados();
        }

        public void sendPosibleWinner(clsCarta carta, string grupo,string nickname) {

            Clients.Group(grupo,Context.ConnectionId).comprobarGanador(carta,nickname);
        }

        public void Ganador(string nickname, string groupname) {

            Clients.Group(groupname).finalizarPartidaPorGanador(nickname);
        }

        public void SalirAbruptamente(string groupName) {

            Clients.Group(groupName,Context.ConnectionId).salirAbriptamente();
        }

        public void Perdedor(string grupo) {

            Clients.Group(grupo,Context.ConnectionId).falloAdivinar();
        }

        public void GanadorPorFallos(string grupo,string nickname) {
        
         Clients.Group(grupo).finalizarPartidaPorFallos(nickname);

        }


        /// <summary>
        /// Elimina una conexion del grupo
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public void LeaveGroup(string groupName)
        {
            int id = maneja.ObtenerIDSala(groupName);
            UsuariosConectados[id] = 0;
            Clients.Group(groupName).abandoPartida();
            Groups.Remove(Context.ConnectionId, groupName);  

        }


        public void LeaveGroupDef(string groupName)
        {
            int id = maneja.ObtenerIDSala(groupName);
            UsuariosConectados[id] = 0;
            Groups.Remove(Context.ConnectionId, groupName);
            
        }
        /// <summary>
        /// Envia el mensaje a los clientes conectados al grupo
        /// </summary>
        /// <param name="message">ChatMessage</param>
        public void SendToGroup(ChatMessage message)
        {
            Clients.Group(message.groupName).agregarMensaje(message);
        }

       
    }
}