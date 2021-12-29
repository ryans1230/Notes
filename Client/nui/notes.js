let notes = [];
let counter = 1;
let currentId = 1;
let currentIndex = 0;

$(function () {
    notes.push({ noteId: counter, text: '' });
    $('#back-button').toggle(false);
    $('#forward-button').toggle(false);
    document.addEventListener('keydown', (event) => {
        const { key } = event;
        if (key === 'Escape') {
            $('#notes').hide();
            sendNUI('close', JSON.stringify({}));
        }
    });

    $('#back-button').click(() => {
        if (currentIndex === 0) {
            return;
        }

        notes[currentIndex].text = $('#textArea').val();

        const prevNote = notes[--currentIndex];
        currentId = prevNote.noteId;
        $('#textArea').val(prevNote.text);
        $('#back-button').toggle(currentIndex > 0);
        $('#forward-button').toggle(true);
    });

    $('#forward-button').click(() => {
        if (currentIndex === notes.length - 1) {
            return;
        }

        notes[currentIndex].text = $('#textArea').val();

        const prevNote = notes[++currentIndex];
        currentId = prevNote.noteId;
        $('#textArea').val(prevNote.text);
        $('#back-button').toggle(true);
        $('#forward-button').toggle(currentIndex < notes.length - 1);
    });

    $('#new-button').click(() => {
        notes[currentIndex] = { noteId: currentId, text: $('#textArea').val() };
        currentId = ++counter;
        notes.push({ noteId: currentId, text: '' });
        $('#textArea').val('');
        currentIndex = notes.length - 1;
        $('#back-button').toggle(true);
        $('#forward-button').toggle(false);
    });

    $('#delete-button').click(() => {
        if (currentIndex === 0) {
            // deleting the first note
            notes.shift();
            if (notes.length > 0) {
                currentId = notes[0].noteId;
                $('#textArea').val(notes[0].text);
            } else {
                currentId = ++counter;
                notes.push({ noteId: currentId, text: '' });
                $('#textArea').val('');
                currentIndex = notes.length - 1;
            }
        } else {
            const prevNote = notes[currentIndex - 1];
            notes.splice(currentIndex, 1);
            currentId = prevNote.noteId;
            $('#textArea').val(prevNote.text);
            currentIndex = notes.findIndex((note) => note.noteId === currentId);
        }
        $('#back-button').toggle(currentIndex > 0);
        $('#forward-button').toggle(currentIndex < notes.length - 1);
    });

    $('#save-button').click(() => {
        if (typeof currentId === 'number') {
            sendNUI('saveNote', JSON.stringify({ text: $('#textArea').val() }));
        } else {
            sendNUI(
                'updateNote',
                JSON.stringify({
                    text: $('#textArea').val(),
                    id: currentId,
                })
            );
        }
        notes[currentIndex] = { noteId: currentId, text: $('#textArea').val() };
        $('#notes').fadeOut();
        setTimeout(() => $('#textArea').val(''), 500);
    });

    window.addEventListener('message', function (event) {
        const data = event.data;
        if (data.type == 'FOCUS') {
            $('#notes').fadeIn();
            $('#textArea').focus();
        } else if (data.type == 'UPDATE') {
            const noteIndex =
                notes.findIndex((note) => note.noteId === data.id);
            if (noteIndex !== -1) {
                // note already loaded
                notes[noteIndex] = { noteId: data.id, text: data.text };
                currentIndex = noteIndex;
            } else {
                notes.push({ noteId: data.id, text: data.text });
                currentIndex = notes.length - 1;
            }
            currentId = data.id;
            $('#textArea').val(data.text);
            $('#notes').fadeIn();
            $('#textArea').focus();
            $('#back-button').toggle(currentIndex > 0);
            $('#forward-button').toggle(currentIndex < notes.length - 1);
        } else if (data.type == 'CLOSE_UI') {
            $('#notes').hide();
        }
    });
});

function sendNUI(name, payload) {
    $.post(`https://${GetParentResourceName()}/${name}`, payload, (datab) => {
        if (datab !== 'ok') {
            console.error(`NUI ERROR: ${datab}`);
        }
    });
}
