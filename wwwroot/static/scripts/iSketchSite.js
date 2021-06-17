document.body.addEventListener('click', function (e) {
    e.path.forEach(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return true;
        }
    });
});