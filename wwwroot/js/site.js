// Scripts personnalisés
$(document).ready(function () {
    // Activer DataTables si présent
    if ($.fn.dataTable) {
        $('#dataTable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/fr-FR.json'
            },
            responsive: true
        });
    }

    // Auto-dissmiss des alertes
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Confirmation de suppression
    $('.delete-confirm').on('click', function (e) {
        return confirm('Êtes-vous sûr de vouloir supprimer cet élément ?');
    });

    // Mise à jour automatique des stocks
    function updateStockIndicators() {
        $('.stock-indicator').each(function () {
            var quantite = parseFloat($(this).data('quantite'));
            var seuil = parseFloat($(this).data('seuil'));
            var progressBar = $(this).find('.progress-bar');
            var percentage = Math.min((quantite / (seuil * 2)) * 100, 100);

            progressBar.css('width', percentage + '%');
            if (quantite < seuil) {
                progressBar.addClass('bg-danger');
                progressBar.removeClass('bg-success');
            } else {
                progressBar.addClass('bg-success');
                progressBar.removeClass('bg-danger');
            }
        });
    }

    updateStockIndicators();

    // Gestion du formulaire de génération de rapport
    $('#generateReport').on('submit', function (e) {
        e.preventDefault();
        var mois = $('#mois').val();
        var annee = $('#annee').val();

        if (mois && annee) {
            window.location.href = '/Rapports/Generate?mois=' + mois + '&annee=' + annee;
        } else {
            alert('Veuillez sélectionner un mois et une année.');
        }
    });
});