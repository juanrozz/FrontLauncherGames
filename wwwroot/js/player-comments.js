// ==================== PROFANITY FILTER ====================

const PROFANITY_API = 'http://us0145ldev01.lat.sitel-world.net/CO/RED/DEV/ProfanityFilterAPI/api/filter/words';

async function checkProfanity(text) {
    try {
        const response = await fetch(`${PROFANITY_API}?userText=${encodeURIComponent(text)}`);
        const result = await response.json();

        if (response.ok) {
            return { passed: true, cleanText: result.data };
        } else {
            return { passed: false, error: result.error };
        }
    } catch (err) {
        console.error('Profanity filter unavailable:', err);
        return { passed: true, cleanText: text };
    }
}

// ==================== AVATAR HELPER ====================

function buildAvatarHTML(userAdp, userName, size) {
    size = size || 20;
    if (userAdp) {
        return `
            <img src="/Store/UserPhoto?adp=${encodeURIComponent(userAdp)}"
                 alt="${escapeHtml(userName)}"
                 class="comment-avatar-img"
                 style="width:${size}px; height:${size}px;"
                 onerror="this.style.display='none'; this.nextElementSibling.style.display='inline-block';" />
            <svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="1.5" style="display:none;">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
            </svg>`;
    }
    return `
        <svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="1.5">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
            <circle cx="12" cy="7" r="4"></circle>
        </svg>`;
}

// ==================== NEW COMMENT MODAL ====================

function openNewCommentModal() {
    document.getElementById('newCommentTitle').value = '';
    document.getElementById('newCommentContent').value = '';
    document.getElementById('newCommentModal').classList.add('active');
}

function closeNewCommentModal() {
    document.getElementById('newCommentModal').classList.remove('active');
}

async function submitNewComment() {
    const title = document.getElementById('newCommentTitle').value.trim();
    const content = document.getElementById('newCommentContent').value.trim();

    if (!content) {
        alert(i18n.alert_write_comment || 'Please write a comment.');
        return;
    }

    // Filtro de profanidad - contenido
    const contentFilter = await checkProfanity(content);
    if (!contentFilter.passed) {
        alert(i18n.alert_profanity || '⚠️ Your comment contains inappropriate language. Please edit it.');
        return;
    }

    // Filtro de profanidad - título
    if (title) {
        const titleFilter = await checkProfanity(title);
        if (!titleFilter.passed) {
            alert(i18n.alert_profanity_title || '⚠️ Your title contains inappropriate language. Please edit it.');
            return;
        }
    }

    try {
        const response = await fetch('/Store/CreateComment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                title: title || null,
                content: contentFilter.cleanText,
                parentId: null
            })
        });

        if (response.status === 401) {
            window.location.href = '/Account/Login?returnUrl=/Store/PlayerComments';
            return;
        }

        if (response.ok) {
            closeNewCommentModal();
            location.reload();
        } else {
            const error = await response.json();
            alert(error.message || (i18n.alert_error_comment || 'Error creating comment.'));
        }
    } catch (err) {
        console.error(err);
        alert(i18n.alert_connection || 'Connection error.');
    }
}

// ==================== REPLY MODAL ====================

function openReplyModal(commentId, previewText) {
    closeDetailModal();

    setTimeout(() => {
        document.getElementById('replyParentId').value = commentId;
        document.getElementById('replyContent').value = '';
        document.getElementById('replyContext').textContent = previewText;
        document.getElementById('replyModal').classList.add('active');
    }, 150);
}

function closeReplyModal() {
    document.getElementById('replyModal').classList.remove('active');
}

async function submitReply() {
    const parentId = parseInt(document.getElementById('replyParentId').value);
    const content = document.getElementById('replyContent').value.trim();

    if (!content) {
        alert(i18n.alert_write_reply || 'Please write a reply.');
        return;
    }

    // Filtro de profanidad
    const filter = await checkProfanity(content);
    if (!filter.passed) {
        alert(i18n.alert_profanity || '⚠️ Your reply contains inappropriate language. Please edit it.');
        return;
    }

    try {
        const response = await fetch('/Store/CreateComment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                parentId,
                content: filter.cleanText,
                title: null
            })
        });

        if (response.status === 401) {
            window.location.href = '/Account/Login?returnUrl=/Store/PlayerComments';
            return;
        }

        if (response.ok) {
            closeReplyModal();
            location.reload();
        } else {
            const error = await response.json();
            alert(error.message || (i18n.alert_error_reply || 'Error posting reply.'));
        }
    } catch (err) {
        console.error(err);
        alert(i18n.alert_connection || 'Connection error.');
    }
}

// ==================== LIKE ====================

async function toggleLike(commentId) {
    try {
        const response = await fetch('/Store/ToggleLike', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ commentId })
        });

        if (response.status === 401) {
            window.location.href = '/Account/Login?returnUrl=/Store/PlayerComments';
            return;
        }

        if (response.ok) {
            const data = await response.json();

            document.querySelectorAll(`.likes-count-${commentId}`).forEach(el => {
                el.textContent = data.likesCount;
            });

            document.querySelectorAll(`[data-comment-id="${commentId}"].like-btn`).forEach(btn => {
                const svg = btn.querySelector('svg');
                if (data.isLiked) {
                    btn.classList.add('liked');
                    svg.setAttribute('fill', '#FFA4A4');
                    svg.setAttribute('stroke', '#FFA4A4');
                } else {
                    btn.classList.remove('liked');
                    svg.setAttribute('fill', 'none');
                    svg.setAttribute('stroke', 'currentColor');
                }
            });

            document.querySelectorAll(`.detail-like-btn-${commentId}`).forEach(btn => {
                const svg = btn.querySelector('svg');
                const span = btn.querySelector('span');
                if (span) span.textContent = data.likesCount;
                if (data.isLiked) {
                    btn.classList.add('liked');
                    if (svg) {
                        svg.setAttribute('fill', '#FFA4A4');
                        svg.setAttribute('stroke', '#FFA4A4');
                    }
                } else {
                    btn.classList.remove('liked');
                    if (svg) {
                        svg.setAttribute('fill', 'none');
                        svg.setAttribute('stroke', 'currentColor');
                    }
                }
            });
        }
    } catch (err) {
        console.error(err);
    }
}

// ==================== DELETE ====================

async function deleteComment(commentId) {
    if (!confirm(i18n.confirm_delete || 'Are you sure you want to delete this comment?')) return;

    try {
        const response = await fetch('/Store/DeleteComment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ commentId })
        });

        if (response.ok) {
            closeDetailModal();
            location.reload();
        } else {
            const error = await response.json();
            alert(error.message || (i18n.alert_error_delete || 'Error deleting comment.'));
        }
    } catch (err) {
        console.error(err);
        alert(i18n.alert_connection || 'Connection error.');
    }
}

// ==================== DETAIL MODAL ====================

let currentDetailId = null;

function getCurrentDetailId() {
    return currentDetailId;
}

async function openDetailModal(commentId) {
    currentDetailId = commentId;

    try {
        const response = await fetch(`/Store/GetReplies/${commentId}`);
        if (!response.ok) return;

        const comment = await response.json();

        document.getElementById('detailTitle').textContent = comment.title || (i18n.comment || 'Comment');
        document.getElementById('detailBody').innerHTML = buildDetailHTML(comment);
        document.getElementById('detailModal').classList.add('active');
    } catch (err) {
        console.error(err);
    }
}

function closeDetailModal() {
    document.getElementById('detailModal').classList.remove('active');
    currentDetailId = null;
}

function buildDetailHTML(comment) {
    const replyText = i18n.reply || 'Reply';
    const deleteText = i18n.delete_btn || 'Delete';
    const byText = i18n.by || 'by';

    let html = `
        <div class="detail-comment-main">
            <p class="detail-content">${escapeHtml(comment.content)}</p>
            <div class="detail-footer">
                <div class="detail-author">
                    <div class="comment-avatar-placeholder">
                        ${buildAvatarHTML(comment.userAdp, comment.userName, 24)}
                    </div>
                    <span>${formatTimeAgo(comment.createdAt)} ${byText} <strong>${escapeHtml(comment.userName)}</strong></span>
                </div>
                <div class="detail-actions">
                    <button class="detail-action-btn" onclick="openReplyModal(${comment.id}, '${escapeJs(comment.content.substring(0, 80))}')">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                        </svg>
                        ${replyText}
                    </button>
                    <button class="detail-action-btn detail-like-btn-${comment.id} ${comment.isLikedByCurrentUser ? 'liked' : ''}"
                            onclick="toggleLike(${comment.id})" data-comment-id="${comment.id}">
                        <svg width="14" height="14" viewBox="0 0 24 24"
                             fill="${comment.isLikedByCurrentUser ? '#FFA4A4' : 'none'}"
                             stroke="${comment.isLikedByCurrentUser ? '#FFA4A4' : 'currentColor'}" stroke-width="1.5">
                            <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                        </svg>
                        <span>${comment.likesCount}</span>
                    </button>
                    ${comment.isOwner ? `
                    <button class="detail-action-btn delete-btn" onclick="deleteComment(${comment.id})">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <polyline points="3 6 5 6 21 6"></polyline>
                            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path>
                        </svg>
                        ${deleteText}
                    </button>` : ''}
                </div>
            </div>
        </div>
    `;

    if (comment.replies && comment.replies.length > 0) {
        const repliesText = i18n.replies || 'Replies';
        html += `<div class="replies-section">`;
        html += `<span class="replies-section-title">${repliesText} (${comment.repliesCount})</span>`;
        html += buildRepliesHTML(comment.replies, 0);
        html += `</div>`;
    }

    return html;
}

function buildRepliesHTML(replies, depth) {
    const byText = i18n.by || 'by';
    const deleteText = i18n.delete_btn || 'Delete';

    let html = '';
    for (const reply of replies) {
        const nestedClass = depth > 0 ? 'reply-item-nested' : '';
        html += `
            <div class="reply-item ${nestedClass}">
                <p class="reply-content">${escapeHtml(reply.content)}</p>
                <div class="reply-footer">
                    <div class="reply-author-info">
                        <div class="comment-avatar-placeholder" style="width:20px; height:20px;">
                            ${buildAvatarHTML(reply.userAdp, reply.userName, 20)}
                        </div>
                        <span class="reply-meta">${formatTimeAgo(reply.createdAt)} ${byText} <strong>${escapeHtml(reply.userName)}</strong></span>
                    </div>
                    <div class="reply-actions">
                        <button class="detail-action-btn" onclick="openReplyModal(${reply.id}, '${escapeJs(reply.content.substring(0, 80))}')">
                            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"></path>
                            </svg>
                        </button>
                        <button class="detail-action-btn detail-like-btn-${reply.id} ${reply.isLikedByCurrentUser ? 'liked' : ''}"
                                onclick="toggleLike(${reply.id})" data-comment-id="${reply.id}">
                            <svg width="12" height="12" viewBox="0 0 24 24"
                                 fill="${reply.isLikedByCurrentUser ? '#FFA4A4' : 'none'}"
                                 stroke="${reply.isLikedByCurrentUser ? '#FFA4A4' : 'currentColor'}" stroke-width="1.5">
                                <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
                            </svg>
                            <span class="likes-count-${reply.id}">${reply.likesCount}</span>
                        </button>
                        ${reply.isOwner ? `
                        <button class="detail-action-btn delete-btn" onclick="deleteComment(${reply.id})">
                            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                <polyline points="3 6 5 6 21 6"></polyline>
                                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path>
                            </svg>
                        </button>` : ''}
                    </div>
                </div>
            </div>
        `;

        if (reply.replies && reply.replies.length > 0) {
            html += buildRepliesHTML(reply.replies, depth + 1);
        }
    }
    return html;
}

// ==================== UTILITIES ====================

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function escapeJs(text) {
    return text.replace(/'/g, "\\'").replace(/"/g, '\\"').replace(/\n/g, ' ');
}

function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);
    const months = Math.floor(days / 30);
    const years = Math.floor(days / 365);

    if (minutes < 1) return i18n.time_just_now || 'just now';
    if (minutes < 60) return `${minutes} ${i18n.time_min || 'min.'} ${i18n.time_ago || 'ago'}`;
    if (hours < 24) return `${hours} ${i18n.time_hrs || 'hrs.'} ${i18n.time_ago || 'ago'}`;
    if (days < 30) return `${days} ${i18n.time_days || 'days'} ${i18n.time_ago || 'ago'}`;
    if (months < 12) return `${months} ${i18n.time_months || 'months'} ${i18n.time_ago || 'ago'}`;
    return `${years} ${i18n.time_years || 'years'} ${i18n.time_ago || 'ago'}`;
}

// Close modals with Escape key
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeNewCommentModal();
        closeReplyModal();
        closeDetailModal();
    }
});

// Close modals clicking outside
document.querySelectorAll('.comment-modal-overlay').forEach(overlay => {
    overlay.addEventListener('click', function (e) {
        if (e.target === this) {
            this.classList.remove('active');
        }
    });
});