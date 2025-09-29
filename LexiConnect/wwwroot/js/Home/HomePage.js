

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
                    btn.addClass("liked").text("👎");
                } else {
                    btn.removeClass("liked").text("👍");
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
