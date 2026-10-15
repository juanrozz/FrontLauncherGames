document.addEventListener('DOMContentLoaded', () => {
    const gameSlides = document.querySelectorAll('.game-slide');

    document.querySelectorAll('.game-item:not(.disabled)').forEach(item => {
        item.addEventListener('click', () => {
            const parent = item.parentNode;
            const items = Array.from(parent.querySelectorAll('.game-item:not(.disabled)'));
            const firstItem = items[0];
            const gameId = item.getAttribute('data-game');

            
            if (item === firstItem) return;

            
            const itemRect = item.getBoundingClientRect();
            const firstRect = firstItem.getBoundingClientRect();
            const diffY = itemRect.top - firstRect.top;

            
            parent.style.pointerEvents = 'none';

            
            item.style.transition = 'transform 0.4s ease';
            item.style.transform = `translateY(-${diffY}px)`;
            item.style.zIndex = '10';

            firstItem.style.transition = 'transform 0.4s ease';
            firstItem.style.transform = `translateY(${diffY}px)`;
            firstItem.style.zIndex = '5';

           
            setTimeout(() => {
                
                item.style.cssText = '';
                firstItem.style.cssText = '';

                
                parent.insertBefore(item, firstItem);

               
                items.forEach(i => i.classList.remove('active'));
                gameSlides.forEach(s => s.classList.remove('active'));

                item.classList.add('active');
                const slide = document.querySelector('.game-slide[data-game="' + gameId + '"]');
                if (slide) slide.classList.add('active');

                
                parent.style.pointerEvents = '';
            }, 420);
        });
    });
});