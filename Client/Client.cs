using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes.Client
{
    public class Client : BaseScript
    {
        internal HashSet<Note> _notes = new HashSet<Note>();
        internal bool _init, _editing;

        public Client()
        {
            TriggerServerEvent("Notes:GetAllNotes");
            RegisterNUICallback("saveNote", SaveNote);
            RegisterNUICallback("updateNote", UpdateNote);
            RegisterNUICallback("close", Close);
        }

        [Command("notes")]
        internal void NotesCommand()
        {
            API.SetNuiFocus(true, true);
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                type = "FOCUS"
            }));
            TriggerEvent("dpEmotes:PlayEmote", "notepad");
            _editing = true;
        }

        [EventHandler("Notes:AllNotes")]
        internal void AllNotesEvent(string data) => _notes = JsonConvert.DeserializeObject<HashSet<Note>>(data);

        [EventHandler("Notes:CloseResourceNUI")]
        internal void ForceCloseNUI()
        {
            _editing = false;
            API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "CLOSE_UI" }));
        }


        [Tick]
        internal async Task MainTick()
        {
            if (!_init)
            {
                await Delay(2000);
                API.SendNuiMessage(JsonConvert.SerializeObject(new
                {
                    type = "RESOURCE_NAME",
                    name = API.GetCurrentResourceName()
                }));
                _init = true;
            }
            if (Game.PlayerPed.IsDead)
            {
                if (_editing)
                {
                    API.SetNuiFocus(false, false);
                    Log("revoking nui focus");
                    ForceCloseNUI();
                    await Delay(1000);
                }
            }
            else
            {
                Vector3 pos = Game.PlayerPed.Position;
                _notes.Where(n => !n.Position.IsZero && World.GetDistance(pos, n.Position) < 20f).ToList().ForEach(n =>
                {
                    World.DrawMarker(MarkerType.HorizontalSplitArrowCircle, n.Position, Vector3.Zero, Vector3.Zero, new Vector3(0.75f, 0.75f, 0.5f), Color.FromArgb(115, 115, 155), false, true);
                    if (World.GetDistance(pos, n.Position) < 2f && !Game.PlayerPed.IsSittingInVehicle())
                    {
                        Screen.DisplayHelpTextThisFrame("~y~E ~w~to read note.\n~y~F ~w~to destroy.");
                        Game.DisableControlThisFrame(0, Control.Enter);
                        Game.DisableControlThisFrame(0, Control.Pickup);
                        Guid noteId = n.NoteId;
                        if (Game.IsControlJustPressed(0, Control.VehicleExit))
                        {
                            TriggerServerEvent("Notes:DeleteNote", noteId.ToString());
                        }
                        else if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            TriggerEvent("dpEmotes:PlayEmote", "notepad");
                            API.SetNuiFocus(true, true);
                            API.SendNuiMessage(JsonConvert.SerializeObject(new
                            {
                                type = "UPDATE",
                                text = n.NoteMessage,
                                id = noteId
                            }));
                        }
                    }
                });
            }
        }

        private CallbackDelegate SaveNote(IDictionary<string, object> data, CallbackDelegate result)
        {
            result(Close(data, result));
            if (data["text"].ToString().Trim() != "")
            {
                Vector3 pos = Game.PlayerPed.Position;
                pos.Z -= 0.75f;
                TriggerServerEvent("Notes:AddNote", pos, data["text"].ToString());
            }
            result("ok");
            return result;
        }

        private CallbackDelegate UpdateNote(IDictionary<string, object> data, CallbackDelegate result)
        {
            result(Close(data, result));
            Vector3 pos = Game.PlayerPed.Position;
            pos.Z -= 0.75f;
            TriggerServerEvent("Notes:UpdateNote", pos, data["id"].ToString(), data["text"].ToString());
            result("ok");
            return result;
        }

        private CallbackDelegate Close(IDictionary<string, object> data, CallbackDelegate result)
        {
            _editing = false;
            API.SetNuiFocus(false, false);
            Log("revoking nui focus");
            TriggerEvent("dpEmotes:StopEmote");
            result("ok");
            return result;
        }

        private void RegisterNUICallback(string msg, Func<IDictionary<string, object>, CallbackDelegate, CallbackDelegate> callback)
        {
            //C# Implementation of the lua RegisterNUICallback
            API.RegisterNuiCallbackType(msg);

            EventHandlers[$"__cfx_nui:{msg}"] += new Action<ExpandoObject, CallbackDelegate>((body, resultCallback) => //All NUI callbacks have event names with the __cfx_nui: prefix
            {
                callback.Invoke(body, resultCallback);
            });
        }

        private void Log(string msg) => Debug.WriteLine($"notes: {msg}");

    }

    public class Note
    {
        public Guid NoteId { get; set; }
        public Vector3 Position { get; set; }
        public string NoteMessage { get; set; }
    }
}
