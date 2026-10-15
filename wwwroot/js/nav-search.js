document.addEventListener('DOMContentLoaded', function () {

    var searchInput = document.getElementById('navSearchInput');
    if (!searchInput) return;

    // Elimina dropdown previo si existe
    var old = document.getElementById('navSearchResults');
    if (old) old.remove();

    // Crea el dropdown en el body
    var dropdown = document.createElement('div');
    dropdown.id = 'navSearchResults';
    dropdown.style.cssText = 'position:fixed;background:#161655;border:1px solid rgba(255,255,255,0.15);border-radius:12px;max-height:250px;overflow-y:auto;z-index:99999;display:none;box-shadow:0 8px 24px rgba(0,0,0,0.6);';
    document.body.appendChild(dropdown);

    // Define las páginas navegables
    var pages = [
        {
            name: 'Store',
            nameEs: 'Tienda',
            icon: 'fa-solid fa-store',
            url: '/'
        },
        {
            name: 'Forums',
            nameEs: 'Foros',
            icon: 'fa-solid fa-comments',
            url: '/Store/Forums'
        },
        {
            name: 'Documentation',
            nameEs: 'Documentación',
            icon: 'fa-solid fa-book',
            url: '/Store/Documentation'
        },
        {
            name: 'Comments',
            nameEs: 'Comentarios',
            icon: 'fa-solid fa-message',
            url: '/Store/PlayerComments'
        }
    ];

    function filterPages(query) {
        if (!query) return [];
        var lowerQuery = query.toLowerCase();
        return pages.filter(function (page) {
            return page.name.toLowerCase().includes(lowerQuery) ||
                page.nameEs.toLowerCase().includes(lowerQuery);
        });
    }

    function positionDropdown() {
        var rect = searchInput.getBoundingClientRect();
        dropdown.style.top = (rect.bottom + 6) + 'px';
        dropdown.style.left = (rect.left - 50) + 'px';
        dropdown.style.width = (rect.width + 80) + 'px';
    }

    function showResults(results) {
        if (results.length === 0) {
            dropdown.style.display = 'none';
            dropdown.innerHTML = '';
            return;
        }

        var html = '';
        results.forEach(function (page) {
            html += '<div class="nav-search-item" data-url="' + page.url + '" style="display:flex;align-items:center;gap:12px;padding:12px 16px;cursor:pointer;transition:background 0.2s;">';
            html += '<i class="' + page.icon + '" style="color:rgba(255,255,255,0.5);font-size:16px;width:20px;text-align:center;"></i>';
            html += '<div style="display:flex;flex-direction:column;">';
            html += '<span style="color:#ffffff;font-size:14px;font-weight:500;">' + page.name + '</span>';
            html += '<span style="color:rgba(255,255,255,0.4);font-size:12px;">' + page.nameEs + '</span>';
            html += '</div>';
            html += '</div>';
        });

        dropdown.innerHTML = html;
        positionDropdown();
        dropdown.style.display = 'block';

        var items = dropdown.querySelectorAll('.nav-search-item');
        items.forEach(function (item) {
            item.addEventListener('click', function () {
                window.location.href = this.getAttribute('data-url');
            });
            item.addEventListener('mouseenter', function () {
                this.style.background = 'rgba(68, 75, 249, 0.25)';
            });
            item.addEventListener('mouseleave', function () {
                this.style.background = 'transparent';
            });
        });
    }

    // Mostrar todas las páginas cuando se enfoca vacío
    searchInput.addEventListener('focus', function () {
        var query = this.value.trim();
        if (query.length === 0) {
            showResults(pages); // Muestra todas
        } else {
            showResults(filterPages(query));
        }
    });

    searchInput.addEventListener('input', function () {
        var query = this.value.trim();
        if (query.length === 0) {
            showResults(pages); // Muestra todas
            return;
        }
        showResults(filterPages(query));
    });

    document.addEventListener('click', function (e) {
        if (!searchInput.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.style.display = 'none';
        }
    });

    window.addEventListener('scroll', function () {
        if (dropdown.style.display === 'block') positionDropdown();
    });

    window.addEventListener('resize', function () {
        if (dropdown.style.display === 'block') positionDropdown();
    });

    // Navegación con teclado
    searchInput.addEventListener('keydown', function (e) {
        var items = dropdown.querySelectorAll('.nav-search-item');
        if (items.length === 0) return;

        var current = dropdown.querySelector('.nav-search-item.highlighted');
        var index = -1;
        if (current) index = Array.from(items).indexOf(current);

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (current) { current.classList.remove('highlighted'); current.style.background = 'transparent'; }
            index = (index + 1) % items.length;
            items[index].classList.add('highlighted');
            items[index].style.background = 'rgba(68, 75, 249, 0.25)';
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (current) { current.classList.remove('highlighted'); current.style.background = 'transparent'; }
            index = (index - 1 + items.length) % items.length;
            items[index].classList.add('highlighted');
            items[index].style.background = 'rgba(68, 75, 249, 0.25)';
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (current) window.location.href = current.getAttribute('data-url');
        } else if (e.key === 'Escape') {
            dropdown.style.display = 'none';
            searchInput.blur();
        }
    });
});