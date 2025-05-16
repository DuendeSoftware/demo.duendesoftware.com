document.addEventListener('DOMContentLoaded', function () {
    $('.clipboard').tooltip({
        trigger: 'click',
        placement: 'top'
    });

    if (ClipboardJS) {
        const clipboard = new ClipboardJS('.clipboard');
        if (clipboard) {
            clipboard.on('success', function (e) {
                $(e.trigger).tooltip('hide')
                    .attr('data-original-title', 'Copied!')
                    .tooltip('show');
                e.clearSelection();

                setTimeout(function () {
                    $(e.trigger).tooltip('hide');
                }, 1500);
            });
        }
    }
});