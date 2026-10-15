document.addEventListener('DOMContentLoaded', function () {

    var searchInput = document.getElementById('gameSearchInput');
    if (!searchInput) return;

    // Elimina cualquier searchResults previo
    var old = document.getElementById('searchResults');
    if (old) old.remove();

    // Crea el dropdown en el body
    var searchResults = document.createElement('div');
    searchResults.id = 'searchResults';
    searchResults.style.cssText = 'position:fixed;background:#161655;border:1px solid rgba(255,255,255,0.15);border-radius:12px;max-height:200px;overflow-y:auto;z-index:99999;display:none;box-shadow:0 8px 24px rgba(0,0,0,0.6);';
    document.body.appendChild(searchResults);

    function getAvailableGames() {
        var games = [];
        var items = document.querySelectorAll('.game-item:not(.disabled)');
        items.forEach(function (item) {
            games.push({
                id: item.getAttribute('data-game'),
                name: item.querySelector('span').textContent
            });
        });
        return games;
    }

    function filterGames(query) {
        if (!query) return [];
        var lowerQuery = query.toLowerCase();
        return getAvailableGames().filter(function (game) {
            return game.name.toLowerCase().includes(lowerQuery);
        });
    }

    function positionDropdown() {
        var rect = searchInput.getBoundingClientRect();
        searchResults.style.top = (rect.bottom + 6) + 'px';
        searchResults.style.left = rect.left + 'px';
        searchResults.style.width = rect.width + 'px';
    }

    function showResults(results) {
        if (results.length === 0) {
            searchResults.style.display = 'none';
            searchResults.innerHTML = '';
            return;
        }

        var html = '';
        results.forEach(function (game) {
            html += '<div class="search-result-item" data-game-id="' + game.id + '" style="display:flex;align-items:center;padding:10px 16px;cursor:pointer;transition:background 0.2s;">';
            html += '<span style="color:#ffffff;font-size:14px;font-weight:500;">' + game.name + '</span>';
            html += '</div>';
        });

        searchResults.innerHTML = html;
        positionDropdown();
        searchResults.style.display = 'block';

        var items = searchResults.querySelectorAll('.search-result-item');
        items.forEach(function (item) {
            item.addEventListener('click', function () {
                selectGame(this.getAttribute('data-game-id'));
            });
            item.addEventListener('mouseenter', function () {
                this.style.background = 'rgba(68, 75, 249, 0.25)';
            });
            item.addEventListener('mouseleave', function () {
                this.style.background = 'transparent';
            });
        });
    }

    function selectGame(gameId) {
        var gameItem = document.querySelector('.game-item[data-game="' + gameId + '"]');
        if (gameItem) {
            gameItem.click();
            document.querySelector('.game-showcase').scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
        searchInput.value = '';
        searchResults.style.display = 'none';
        searchResults.innerHTML = '';
    }

    searchInput.addEventListener('input', function () {
        var query = this.value.trim();
        if (query.length === 0) {
            searchResults.style.display = 'none';
            searchResults.innerHTML = '';
            return;
        }
        showResults(filterGames(query));
    });

    searchInput.addEventListener('focus', function () {
        var query = this.value.trim();
        if (query.length > 0) {
            showResults(filterGames(query));
        }
    });

    document.addEventListener('click', function (e) {
        if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
            searchResults.style.display = 'none';
        }
    });

    window.addEventListener('scroll', function () {
        if (searchResults.style.display === 'block') positionDropdown();
    });

    window.addEventListener('resize', function () {
        if (searchResults.style.display === 'block') positionDropdown();
    });

    searchInput.addEventListener('keydown', function (e) {
        var items = searchResults.querySelectorAll('.search-result-item');
        if (items.length === 0) return;

        var current = searchResults.querySelector('.search-result-item.highlighted');
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
            if (current) selectGame(current.getAttribute('data-game-id'));
        } else if (e.key === 'Escape') {
            searchResults.style.display = 'none';
            searchInput.blur();
        }
    });
});