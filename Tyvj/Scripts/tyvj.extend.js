var page = 0;
var lock = false;
var isIE = !!window.ActiveXObject;
var isIE6 = isIE && !window.XMLHttpRequest;
var isIE8 = isIE && !!document.documentMode;
var isIE7 = isIE && !isIE6 && !isIE8;
var isIE678 = isIE6 || isIE7 || isIE8;
var id = null;


function Load() {
    if (lock) return;
    lock = true;
    if ($("#btnMore").length > 0) {
        $("#btnMore").html("加载中...");
    }

    LoadContests();
    LoadProblems();
    LoadStatuses();
    LoadTopics();
    LoadReplies();
}

function Lock() {
    lock = true;
    if ($("#btnMore").length > 0) {
        $("#btnMore").html("没有更多内容了 ╮(╯▽╰)╭");
        $("#btnMore").removeClass("enabled");
    }
}

function LoadContests() {
    if ($("#lstContests").length > 0) {
        $.getJSON("/Contest/GetContests", {
            page: page,
            format: format,
            title: title,
            rnd: Math.random()
        }, function (contests) {
            $("#btnMore").html("点击加载更多");
            if (contests.length < 10) {
                Lock();
                if (contests.length == 0)
                    return;
            }
            if (contests.length == 0) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
            for (var i = 0; i < contests.length; i++) {
                if (contests[i] == null) continue;
                var color = contests[i].StatusAsInt != 0 ? "#333" : "green";
                $("#lstContests").append('<tr><td class="c1">'
                                                 + '<div class="title"><a href="/Contest/' + contests[i].ID + '">' + contests[i].Title + '</a></div>'
                                                 + '<div class="footer"><span>赛制：' + contests[i].Format + ' / 参与人数：' + contests[i].Join + ' / 开始时间：' + contests[i].Begin + ' / 时长：' + contests[i].Duration + '</span></div></td>'
                                                 + '<td class="c2"><span style="color:' + color + '">' + contests[i].Status + '</span></td></tr>');
            }
            lock = false;
            page++;
        });
    }
}
function LoadProblems() {
    if ($("#lstProblems").length > 0) {
        $.getJSON("/Problem/GetProblems", {
            page: page,
            morethan: morethan,
            lessthan: lessthan,
            title: title,
            tags: tags,
            rnd: Math.random()
        }, function (problems) {
            $("#btnMore").html("点击加载更多");
            if (problems.length < 10) {
                Lock();
                if (problems.length == 0)
                    return;
            }

            for (var i = 0; i < problems.length; i++) {
                if (problems[i] == null) continue;
                var contest_label = "";
                if (problems[i].ContestTitle != null && problems[i].ContestTitle != "") {
                    contest_label = '<span class="label ' + css[problems[i].ContestID % 5] + '">' + problems[i].ContestTitle + '</span>';
                }
                if (!Signed)
                    $("#lstProblems").append('<tr data-id="' + problems[i].ID + '" class="tyvj-list-body-tr even"><td class="tyvj-list-td vjlc3" style="text-align:left"><div class="wrap"><div class="title">'
                                                 + '<a href="/p/' + problems[i].ID + '" target="_self" class="pid">P' + problems[i].ID + '</a> '
                                                 + '<a href="/p/' + problems[i].ID + '" target="_self">' + problems[i].Title + '</a></div></div></td>'
                                                 + '<td class="tyvj-list-td vjlc4">' + problems[i].Accepted + '</td><td class="tyvj-list-td vjlc5">' + problems[i].Submitted + '</td><td class="tyvj-list-td vjlc6">' + problems[i].Ratio + '%</td></tr>');
                else {
                    var flag = '<td class="tyvj-list-td vjlc1"></td>';
                    if (problems[i].Flag == 1) {
                        flag = '<td class="tyvj-list-td vjlc1"><span style="color:dodgerblue">PEND</span></td>';
                    }
                    else if (problems[i].Flag == 2) {
                        flag = '<td class="tyvj-list-td vjlc1"><span style="color:green">AC</span></td>';
                    }
                    $("#lstProblems").append('<tr data-id="' + problems[i].ID + '" class="tyvj-list-body-tr even">' + flag + '<td class="tyvj-list-td vjlc3" style="text-align:left"><div class="wrap"><div class="title">'
                                                 + '<a href="/p/' + problems[i].ID + '" target="_self" class="pid">P' + problems[i].ID + '</a> '
                                                 + '<a href="/p/' + problems[i].ID + '" target="_self">' + problems[i].Title + '</a></div></div></td>'
                                                 + '<td class="tyvj-list-td vjlc4">' + problems[i].Accepted + '</td><td class="tyvj-list-td vjlc5">' + problems[i].Submitted + '</td><td class="tyvj-list-td vjlc6">' + problems[i].Ratio + '%</td></tr>');
                }
            }
            lock = false;
            page++;
        });
    }
}
function LoadStatuses() {
    if ($("#lstStatuses").length > 0) {
        $.getJSON("/Status/GetStatuses", {
            page: page,
            contestid: contest_id,
            problemid: problem_id,
            username: username,
            result: result,
            rnd: Math.random()
        }, function (statuses) {
            $("#btnMore").html("点击加载更多");
            if (statuses.length < 10) {
                Lock();
                if (statuses.length == 0)
                    return;
            }
            for (var i = 0; i < statuses.length; i++) {
                if (statuses[i] == null) continue;
                $("#lstStatuses").append('<tr id="s-' + statuses[i].ID + '" class="tyvj-list-body-tr">'
                                                 + '<td class="tyvj-list-td tyvjlc1"><a href="/Status/' + statuses[i].ID + '" class="judgeState' + statuses[i].ResultAsInt + '">' + statuses[i].Result + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc2" style="padding:0;border-left:1px solid #ccc"><div class="pg"><div class="pglt" style="width:' + statuses[i].Score + '%;"></div><div class="text">' + statuses[i].Score + '</div></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc22">' + statuses[i].TimeUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc23" style="border-right:1px solid #ccc">' + statuses[i].MemoryUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc3" style="text-align:left"><div class="wrap"><span class="c"><a href="/p/' + statuses[i].ProblemID + '" target="_self">P' + statuses[i].ProblemID + '&nbsp;' + statuses[i].ProblemTitle + '</a></span></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc4" style="border-right:1px solid #ccc"><a href="/User/' + statuses[i].UserID + '" target="_blank" class="user">' + statuses[i].Username + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc5" style="border-right:1px solid #ccc">' + statuses[i].Language + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc7">' + statuses[i].Time + '</td></tr>');
            }
            lock = false;
            page++;
        });
    }
}

function LoadTopics() {
    if ($("#lstTopics").length > 0) {
        $.getJSON("/Topic/GetTopics", {
            page: page,
            forumid: forum_id,
            rnd: Math.random()
        }, function (topics) {
            $("#btnMore").html("点击加载更多");
            if (topics.length < 10) {
                Lock();
                if (topics.length == 0)
                    return;
            }
            for (var i = 0; i < topics.length; i++) {
                if (topics[i] == null) continue;
                $("#lstTopics").append('<tr class=""><td class="c1"><img class="face" src=' + topics[i].Gravatar + '></img>'
                                                 + '</td><td class="c2">'
                                                 + '<div class="title"><a href="/Topic/' + topics[i].ID + '">' + topics[i].Title + '</a></div>'
                                                 + '<div class="footer">作者：<a href="/User/' + topics[i].UserID + '">' + topics[i].Nickname + '</a> / ' + topics[i].Time + ' 发表在 <a href="/Forum/' + topics[i].ForumID + '">' + topics[i].ForumTitle + '</a> ' + (topics[i].HasReply ? '最新回复：<a href="/User/' + topics[i].LastReplyUserID + '">' + topics[i].LastReplyNickname + '</a> @' + topics[i].LastReplyTime : '') + '</div>'
                                                 + '</td>'
                                                 + '<td class="c3">' + topics[i].ForumID + '</td></tr>');
            }
            lock = false;
            page++;
        });
    }
}


function LoadReplies() {
    if ($("#lstReplies").length > 0) {
        $.post("/Reply/GetReplies/", {
            id:id,
            page: page,
            rnd: Math.random()
        }, function (replies) {
            if (replies.length < 20) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
            $("#lstReplies").append(replies);

            // CKEditor高亮
            $('.ckeditor-code').each(function () {
                $(this).html('<code>' + $(this).html() + '</code>');
                $(this).removeClass('ckeditor-code');
            });

            PostReplyBinding();
            lock = false;
            page++;
            $("#iconLoading").hide();
            if (window.location.hash != null && $(window.location.hash).length > 0)
                $('html,body').scrollTop($(window.location.hash).offset().top + 170);
        }).complete(function () {
            // CKEditor高亮
            $('pre code').each(function (i, block) {
                if (navigator.userAgent.indexOf("MSIE") == -1) {
                    hljs.highlightBlock(block);
                }
            });
        });
    }
}

$(document).ready(function () {
    Load();
    $(window).scroll(function () {
        totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop());
        if ($(document).height() <= totalheight) {
            Load();
        }
    });
    $("#btnLoadCodeEditBox").click(function () {
        $.colorbox({ inline: true, width: "700px", href: "#CodeEditBox", onComplete: function () { editor.refresh(); } });
    });
    function RefreshHighLight() {
        var language_id = $("#lstLanguages").val();
        switch (parseInt(language_id)) {
            case 0:
                editor.setOption('mode', 'text/x-csrc');
                break;
            case 1:
                editor.setOption('mode', 'text/x-c++src');
                break;
            case 2:
                editor.setOption('mode', 'text/x-c++src');
                break;
            case 3:
                editor.setOption('mode', 'text/x-java');
                break;
            case 4:
                editor.setOption('mode', 'text/x-pascal');
                break;
            case 5:
                editor.setOption('mode', 'text/x-python');
                break;
            case 6:
                editor.setOption('mode', 'text/x-python');
                break;
            case 7:
                editor.setOption('mode', 'text/x-ruby');
                break;
            case 8:
                editor.setOption('mode', 'text/x-csharp');
                break;
            case 9:
                editor.setOption('mode', 'text/x-vb');
                break;
            default: break;
        }
        editor.refresh();
    }
    if ($("#editor").length > 0) {
        editor = CodeMirror.fromTextArea(document.getElementById("editor"), {
            lineNumbers: true,
            matchBrackets: true,
            indentUnit: 4,
            smartIndent: false,
            mode: "text/x-c++src",
            theme: "neat"
        });
        RefreshHighLight();
        $("#lstLanguages").change(function () {
            RefreshHighLight();
        });
    }
    $("#btnClearCodeBox").click(function () {
        editor.setValue("");
    });
    $("#btnSubmitCode").click(function () {
        $.colorbox({ html: '<h3>评测结果</h3><p>正在等待系统验证及分配评测资源...</p>', width: '700px' });
        $("#editor").val(editor.getValue());
        $.post("/Status/Create", $("#frmSubmitCode").serialize(), function (data) {
            JudgeResultAsInt = 100;
            JudgeResult = "";
            if (data == "Problem not existed") {
                CastMsg("不存在这道题目！");
                $.colorbox.close();
            }
            else if (data == "Insufficient permissions") {
                CastMsg("权限不足！");
                $.colorbox.close();
            }
            else if (data == "Locked") {
                CastMsg("锁定题目后不能提交！");
            }
            else if (data == "Wrong phase") {
                CastMsg("本阶段不允许提交评测！");
            }
            else if (data == "No Online Judger") {
                CastMsg("评测机离线！当有可用评测资源时将评测本记录。");
            }
            else if (data == "OI") {
                $.colorbox.close();
                CastMsg("提交成功");
            }
            else {
                RealTimeStatusID = parseInt(data);
                if (isIE6) window.location = "/Status/" + RealTimeStatusID;
                $.colorbox({ html: '<h3>评测结果</h3><p>正在评测...</p>', width: '700px' });
            }
        });
    });

    //SignalR
    UserHub = $.connection.userHub;
    UserHub.client.onStatusChanged = function (status) {
        if ($("#lstStatuses").length > 0) {
            if ($("#s-" + status.ID) > 0) {
                $("#s-" + status.ID).html('<td class="tyvj-list-td tyvjlc1"><a href="/Status/' + status.ID + '" class="judgeState' + status.ResultAsInt + '">' + status.Result + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc2" style="padding:0;border-left:1px solid #ccc"><div class="pg"><div class="pglt" style="width:' + status.Score + '%;"></div><div class="text">' + status.Score + '</div></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc22">' + status.TimeUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc23" style="border-right:1px solid #ccc">' + status.MemoryUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc3" style="text-align:left"><div class="wrap"><span class="c"><a href="/p/' + status.ProblemID + '" target="_self">P' + status.ProblemID + '&nbsp;' + status.ProblemTitle + '</a></span></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc4" style="border-right:1px solid #ccc"><a href="/User/' + status.UserID + '" target="_blank" class="user">' + status.Username + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc5" style="border-right:1px solid #ccc">' + status.Language + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc7">' + status.Time + '</td>');
            }
            else {
                $("#lstStatuses").prepend('<tr id="s-' + status.ID + '" class="tyvj-list-body-tr">'
                                                 + '<td class="tyvj-list-td tyvjlc1"><a href="/Status/' + status.ID + '" class="judgeState' + status.ResultAsInt + '">' + status.Result + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc2" style="padding:0;border-left:1px solid #ccc"><div class="pg"><div class="pglt" style="width:' + status.Score + '%;"></div><div class="text">' + status.Score + '</div></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc22">' + status.TimeUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc23" style="border-right:1px solid #ccc">' + status.MemoryUsage + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc3" style="text-align:left"><div class="wrap"><span class="c"><a href="/p/' + status.ProblemID + '" target="_self">P' + status.ProblemID + '&nbsp;' + status.ProblemTitle + '</a></span></div></td>'
                                                 + '<td class="tyvj-list-td tyvjlc4" style="border-right:1px solid #ccc"><a href="/User/' + status.UserID + '" target="_blank" class="user">' + status.Username + '</a></td>'
                                                 + '<td class="tyvj-list-td tyvjlc5" style="border-right:1px solid #ccc">' + status.Language + '</td>'
                                                 + '<td class="tyvj-list-td tyvjlc7">' + status.Time + '</td></tr>');
            }
        }
    }
    $.connection.hub.start();
});