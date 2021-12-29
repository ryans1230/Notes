using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Notes.Server
{
    public class Server : BaseScript
    {
        internal readonly List<Note> _notes = new List<Note>();

        [EventHandler("Notes:GetAllNotes")]
        internal void GetAllNotesEvent([FromSource] Player p) => p.TriggerEvent("Notes:AllNotes", JsonConvert.SerializeObject(_notes));

        [EventHandler("Notes:AddNote")]
        internal void AddNoteEvent([FromSource] Player p, Vector3 pos, string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Guid id = Guid.NewGuid();
            Debug.WriteLine($"{p.Name} (#{p.Handle}) created note #{id} with the text: {string.Join("{RETURN}", lines)}");
            _notes.Add(new Note
            {
                NoteId = id,
                NoteMessage = text,
                Position = pos
            });
            TriggerLatentClientEvent("Notes:AllNotes", 5000, JsonConvert.SerializeObject(_notes));
        }

        [EventHandler("Notes:UpdateNote")]
        internal void UpdateNoteEvent([FromSource] Player p, Vector3 pos, string nId, string text)
        {
            Guid noteId = Guid.Parse(nId);
            if (_notes.FirstOrDefault(n => n.NoteId == noteId) != null)
            {
                Note note = _notes.FirstOrDefault(n => n.NoteId == noteId);
                note.NoteMessage = text;
                note.Position = pos;

                string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                Debug.WriteLine($"{p.Name}({p.Handle}) updated note #{noteId} with the text: {string.Join("{RETURN}", lines)}");
            }
            else
            {
                AddNoteEvent(p, pos, text);
            }
            TriggerLatentClientEvent("Notes:AllNotes", 5000, JsonConvert.SerializeObject(_notes));
        }

        [EventHandler("Notes:DeleteNote")]
        internal void DeleteNoteEvent([FromSource] Player p, string nId)
        {
            Guid noteId = Guid.Parse(nId);
            if (_notes.FirstOrDefault(n => n.NoteId == noteId) != null)
            {
                Note note = _notes.FirstOrDefault(n => n.NoteId == noteId);
                Debug.WriteLine($"{p.Name} deleted note #{noteId}");
                _notes.Remove(note);
            }

            TriggerLatentClientEvent("Notes:AllNotes", 5000, JsonConvert.SerializeObject(_notes));
        }
    }

    public class Note
    {
        public Guid NoteId { get; set; }
        public Vector3 Position { get; set; }
        public string NoteMessage { get; set; }

    }
}
