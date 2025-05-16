document.addEventListener('DOMContentLoaded', function () {
    let refresh = document
        .querySelector("meta[http-equiv=refresh]");
    if (refresh) {
        window.location.href = refresh.getAttribute("data-url");
    }    
});
