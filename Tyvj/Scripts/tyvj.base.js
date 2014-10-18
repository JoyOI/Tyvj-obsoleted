$(document).ready(function () {
    $(".checkbox").change(function () {
        if ($(this).is(':checked'))
            $(this).addClass("checked");
        else
            $(this).removeClass("checked");
    });
});

(function (window, undefined) {

    window.popupLogin = function () {
        $("#nav-login").addClass("tyvj-nav-item-hover");
        var pos = $(this).position();
        pos.left = 0;
        pos.left -= 60;
        pos.left -= $('.login-form').outerWidth();
        pos.left += $(window).width() - ($(window).width() - 960) / 2;
        pos.top = 43;
        $('.login-form').css(pos).fadeIn('fast');

        setTimeout(function () { $('.role-login-user').focus(); }, 0);
        $('.role-show-login-form').addClass('page-header-item-hover');
        return false;
    }

    window.hideLogin = function () {
        $('.role-show-login-form').removeClass('page-header-item-hover');
        $('.login-form').stop(true, true).fadeOut('fast');
    }

    $(document).ready(function () {
        // login form
        if ($('.login-form').length > 0) {
            $('.role-show-login-form').click(function () {
                return false;
            });
            $(document).on('click', function (e) {
                if ($(e.target).parents('.login-form').length > 0) return;
                if ($(e.target).parents('#btnShowLogin').length > 0) return;
                hideLogin();
                $("#nav-login").removeClass("tyvj-nav-item-hover");
            });
        }
    });
})(window);

function CastMsg(msg) {
    $(document.body).append("<div class='show_public_tips'><p class='show_public_tips_text'>" + msg + "</p></div>");
    $(".show_public_tips").fadeIn();
    setTimeout(function () {
        $(".show_public_tips").fadeOut();
        setTimeout(function () {
            $(".show_public_tips").remove();
        }, 1000);
    }, 2000);
}