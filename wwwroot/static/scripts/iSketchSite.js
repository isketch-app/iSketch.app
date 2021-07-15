document.body.addEventListener('click', function (e) {
    e.path.forEach(function (t) {
        if (t.classList != undefined && t.classList.contains('CButton')) {
            t.classList.add('clicked');
            return true;
        }
    });
});

document.getElementById('is_body').addEventListener('scroll', function (e) {
    if (e.srcElement.scrollTop > 0) {
        e.srcElement.classList.add('scrolled');
    } else {
        e.srcElement.classList.remove('scrolled');
    }
});