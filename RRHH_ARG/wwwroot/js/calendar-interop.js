var calendarInstance;

function loadCalendar(events) {
    console.log("📦 Renderizando calendario con " + events.length + " eventos.");

    var calendarEl = document.getElementById('calendar');
    if (!calendarEl) return;

    if (calendarInstance) {
        calendarInstance.destroy();
    }

    var today = new Date();
    var oneYearLater = new Date();
    oneYearLater.setFullYear(today.getFullYear() + 1);
    var startRange = new Date(today.getFullYear(), today.getMonth(), 1);

    var isMobile = window.innerWidth < 768;

    calendarInstance = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        showNonCurrentDates: false,  // ✅ oculta días de otros meses
        fixedWeekCount: false,

        headerToolbar: {
            left: 'prev',
            center: 'title',
            right: 'next'
        },

        validRange: {
            start: startRange,
            end: oneYearLater
        },

        events: events,
        locale: 'es',
        height: 'auto',

        // ✅ En móvil, mostrar solo el número del día sin texto extra
        dayHeaderFormat: isMobile
            ? { weekday: 'narrow' }   // "L M X J V S D"
            : { weekday: 'short' },   // "Lun Mar Mié..."

        // ✅ En móvil, limitar eventos visibles por día para no desbordarse
        dayMaxEvents: isMobile ? 2 : 3,

        eventDidMount: function (info) {
            info.el.setAttribute('title', info.event.title);

            if (typeof bootstrap !== 'undefined') {
                try {
                    new bootstrap.Tooltip(info.el, {
                        title: info.event.title,
                        placement: 'top',
                        trigger: 'hover',
                        container: 'body'
                    });
                } catch (error) {
                    console.warn("⚠️ Falló el tooltip: " + info.event.title, error);
                }
            }
        },

        eventClick: function (info) {
            info.jsEvent.preventDefault();
            if (info.event.url && info.event.url !== '#') {
                window.location.href = info.event.url;
            }
        },
    });

    calendarInstance.render();
}

// ✅ Re-renderizar al rotar pantalla o redimensionar
var resizeTimer;
window.addEventListener('resize', function () {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(function () {
        if (calendarInstance) {
            calendarInstance.updateSize();
        }
    }, 300);
});

function destroyCalendar() { if (calendarInstance) { calendarInstance.destroy(); } }
function addEventToCalendar(event) { if (calendarInstance) { calendarInstance.addEvent(event); } }