

////$(document).on("click", ".like-button", function () {
////    var btn = $(this);
////    var docId = btn.data("id");

////    $.ajax({
////        url: '/Document/ToggleLike',
////        type: 'POST',
////        data: {
////            id: docId,
////            isLike: true, // gửi true khi bấm like
////            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
////        },
////        success: function (res) {
////            if (res.success) {
////                // cập nhật số like ở span completion-percentage
////                btn.closest(".top-document-card").find(".completion-percentage").text(res.likeCount);
////            } else {
////                alert(res.message);
////            }
////        },
////        error: function () {
////            alert("Có lỗi xảy ra khi like tài liệu");
////        }
////    });
////});


//$(document).on("click", ".like-button", function () {
//    var btn = $(this);
//    var docId = btn.data("id");
//    var isLiked = btn.hasClass("liked"); // kiểm tra trạng thái hiện tại
//    var token = $('input[name="__RequestVerificationToken"]').val();

//    $.ajax({
//        url: '/Document/ToggleLike',
//        type: 'POST',
//        data: {
//            id: docId,
//            isLike: !isLiked, // đảo trạng thái
//            __RequestVerificationToken: token
//        },
//        success: function (res) {
//            if (res.success) {
//                btn.closest(".top-document-card").find(".completion-percentage").text(res.likeCount);

//                // đổi trạng thái nút
//                if (isLiked) {
//                    btn.removeClass("liked").text("👍 ");
//                } else {
//                    btn.addClass("liked").text("👎 ");
//                }
//            } else {
//                alert(res.message);
//            }
//        },
//        error: function () {
//            alert("Có lỗi xảy ra khi like tài liệu");
//        }
//    });
//});

$(document).on("click", ".like-button", function () {
    var btn = $(this);
    var docId = btn.data("id");
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Document/ToggleLike',
        type: 'POST',
        data: {
            id: docId,
            __RequestVerificationToken: token
        },
        success: function (res) {
            if (res.success) {
                btn.closest(".top-document-card")
                    .find(".completion-percentage")
                    .text(res.likeCount);

                if (res.isLiked) {
                    btn.addClass("liked");
                    // Show filled thumbs up (liked state)
                    btn.html('<svg version="1.0" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 64 64" width="24" height="24" enable-background="new 0 0 64 64" xml:space="preserve" fill="#000000"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <circle fill="#231F20" cx="7" cy="57" r="1"></circle> <g> <path fill="#231F20" d="M14,26c0-2.212-1.789-4-4-4H4c-2.211,0-4,1.788-4,4v34c0,2.21,1.789,4,4,4h6c2.211,0,4-1.79,4-4V26z M7,60 c-1.657,0-3-1.344-3-3c0-1.658,1.343-3,3-3s3,1.342,3,3C10,58.656,8.657,60,7,60z"></path> <path fill="#231F20" d="M64,28c0-3.314-2.687-6-6-6H41l0,0h-0.016H41l2-18c0.209-2.188-1.287-4-3.498-4h-4.001 C33,0,31.959,1.75,31,4l-8,18c-2.155,5.169-5,6-7,6v30.218c1.203,0.285,2.714,0.945,4.21,2.479C23.324,63.894,27.043,64,29,64h23 c3.313,0,6-2.688,6-6c0-1.731-0.737-3.288-1.91-4.383C58.371,52.769,60,50.577,60,48c0-1.731-0.737-3.288-1.91-4.383 C60.371,42.769,62,40.577,62,38c0-1.731-0.737-3.288-1.91-4.383C62.371,32.769,64,30.577,64,28z"></path> </g> </g> </g></svg>');
                } else {
                    btn.removeClass("liked");
                    // Show outline thumbs up (unliked state)
                    btn.html('<svg version="1.0" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 64 64" width="24" height="24" enable-background="new 0 0 64 64" xml:space="preserve" fill="#000000"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <g> <circle fill="#231F20" cx="7" cy="35" r="1"></circle> <g> <path fill="#231F20" d="M0,4c0-2.211,1.789-4,4-4h6c2.211,0,4,1.789,4,4v34c0,2.211-1.789,4-4,4H4c-2.211,0-4-1.789-4-4V4z M7,38 c1.657,0,3-1.343,3-3s-1.343-3-3-3s-3,1.343-3,3S5.343,38,7,38z"></path> <path fill="#231F20" d="M64,36c0,3.313-2.687,6-6,6H41l0,0h-0.016H41l2,18c0.209,2.187-1.287,4-3.498,4h-4.001 C33,64,31.959,62.25,31,60l-8-18c-2.155-5.17-5-6-7-6V5.781c1.203-0.285,2.714-0.945,4.21-2.479C23.324,0.105,27.043,0,29,0h23 c3.313,0,6,2.687,6,6c0,1.73-0.737,3.287-1.91,4.382C58.371,11.23,60,13.422,60,16c0,1.73-0.737,3.287-1.91,4.382 C60.371,21.23,62,23.422,62,26c0,1.73-0.737,3.287-1.91,4.382C62.371,31.23,64,33.422,64,36z"></path> </g> </g> </g></svg>');
                }
            } else {
                alert(res.message);
            }
        },
        error: function () {
            alert("Có lỗi xảy ra khi like tài liệu");
        }
    });
});
