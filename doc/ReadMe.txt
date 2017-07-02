== Line 3217
[..
var cl = $(this).attr("href").replace("/client", "");
newwindow = window.open("http://cdn.media.dafatouzhu.org/prod/macau-live/loader/l.html?t=" + settings.integration.icoreTimeout + "&l=" + Drupal.settings.pathPrefix.replace("/", ""), cl.toUpperCase() + "GameWindow", $.param(settings.matterhorn.popup_window.specs).split("&").join(",")), $.get(Drupal.settings.basePath + Drupal.settings.pathPrefix + "icore/get-lobby-url/" + cl, function(data) {
	"" != data.url && newwindow.location.replace(data.url)
})..]

[..
newwindow = window.open("http://cdn.media.dafatouzhu.org/prod/macau-live/loader/l.html?t=30&l=", "AGGameWindow", "width=800,height=600,top=0,left=0,toolbar=0,location=0,status=0,menubar=0,scrollbars=1,resizable=1");
jQuery.get("/vn/live-dealer/icore/get-lobby-url/ag", function(data) {
	"" != data.url && newwindow.location.replace(data.url)
});..]

http://gci.agad.mstrom-uno.com:81/forwardGame.do?params=F36BBD1DCB90E0E1B27CD91440DEB976AA5D70EF9269BF655AE478CF0CF2630D234290D179BA74BD40C8FE463F8604638A0DCAFE2ABFF2118AB64624C0C2C863A87E63137C1786363909A692DC5120A02D37F9311540548CC257C781D60B3138A582F9AC722C2CE5069C684E90BAE144DCAA77AF6B083A15A2B5EAD110F72BDDB2EDF28D4D95F372D207018373E22DC80559B6CBD700069D6CF7A774DC096586C03B529914B8FCC86CBFB909076CE0BDD5AB8E3A624315867B1315903744F565&key=884198cae18acd47a88954dcd1c763b6

== Skype
https://msdn.microsoft.com/en-us/library/windows/desktop/ms644936%28v=vs.85%29.aspx
mày print hình ảnh muốn cho dung lượng nhẹ thì:- nếu đang là Image<,> x thì gọi x.Bitmap.Save(....) là nó ra có mấy kb thôi, lưu dạng jpg hình như nó nhẹ hơn png
nói chung là dung Bitmap(System).Save(*.jpg) thì nó max nhẹ
(200 + 400 + 800) + (200 + 400 + 800) * 10 + (200 + 400) * 100 ~= 75.000 / 200 ~= 375

== JS
console.clear();
/* -: var input = "\
9	8	7	0		3	3	0	8	4	6	6	8	6	8	9	3	7	5																																																									\n\
5	0	3	6		0	6	6	4	4	1	6	2	7	8	0	4	7	4																																																									\n\
																																																																											\n\
1	0	4	3	1	1	7	8	0	1	0	6	5	6	2	8																																																												\n\
5	6	5	7	6	4	6	0	6	8	9	8	1	4	4	3																																																												\n\
																																																																											\n\
7	6	0	4		9	9	1	7	2	3	4	1	3	8	7	0	5	7	4	9	0	8	2	5	3	0																																																	\n\
3	5	8	2		4	2	0	8	1	4	3	1	7	4	6	3	3	2	4	6	9	0	9	3	3	9																																																	\n\
																																																																											\n\
3	6	6	2	2	4	8	6	4	8	5	9	5	6	7	5	5	9	4	7		9	5	9	7	7	7	0	6	6	4	4	4	6	2	1	2	2		8	9	8	6																																	\n\
1	8	4	7	3	9	7	9	9	0	9	2	8	1	7	5	9	1	5	7		0	7	5	2	6	3	8	8	2	7	6	8	9	9	4	9	0		7	0	0	8																																	\n\
																																																																											\n\
8	7	7	6		4	9	6	7	6	8	2	3	8	7	5	5	3	1	8	1	3	6	0	9	5	0	0	0	3	6	9	9	7	2	9	6																																							\n\
2	7	9	3		9	9	8	0	6	0	5	8	6	6	5	0	8	0	3	1	8	0	9	9	1	7	0	6	7	8	2	2	7	9	1	4																																							\n\
																																																																											\n\
0	9	3	6	0	3	4	5	1	0	9	3	4																																																															\n\
6	3	5	7	9	5	9	1	8	0	7	9	9																																																															\n\
																																																																											\n\
5	9	6	5	8	6	7	9	7	9	8	8	2	9	7		6	2	6	3	5	3	0	7	5	2	9	8	1	7	9	7	8	3	6	6	0	0	7	9	1	9	5	9		9	3	7	8	3	0	9	3	3	5	1	5	5	6	8	4	9	0	1	3	4	9	6	8	1						\n\
9	9	6	6	0	0	5	5	4	6	3	4	7	8	9		7	7	8	9	9	6	5	6	6	6	9	1	0	5	8	1	9	1	6	6	7	0	9	7	8	7	3	4		3	1	2	4	7	3	7	3	8	3	3	6	1	6	6	3	6	9	6	1	9	4	9	9	8						\n\
																																																																											\n\
	8	6	6	6	8	4	5	7	9	3	8	6	3	8		8	5	9	5	5	7	6	9	5	6	0	0	9	0	6	5	7	4	9	4	1		7	8	0	6	7	9	9	6	6	6	8	0		7	4																							\n\
	1	8	8	1	9	2	8	5	6	6	3	8	0	4		7	6	0	4	9	0	8	1	1	5	2	3	5	3	1	4	7	1	0	7	8		4	4	9	8	2	4	4	1	1	7	3	9		0	5																							\n\
";
var lines = [];
input.split("\n").forEach(function (line, index, array) {
    line = line.replace(/\t+$/gi, "");
    if ("" != line) { lines.push(line); }
});
var times = [];
for (var i = 0; i < lines.length; i = i + 2) {
    var arr_b = lines[i].split("\t");
    var arr_p = lines[i + 1].split("\t");
    times.push({ Matches: [] });
    for (var j = 0; j < arr_b.length; j++) {
        times[times.length - 1].Matches.push({ B: arr_b[j], P: arr_p[j] });
    }
}*/
/* -: var pairs = [];
for (var time_i = 0; time_i < times.length; time_i++) {
    var matches_i = times[time_i].Matches;
    for (var match_i = 0; match_i < matches_i.length - 1; match_i++) {
        var match_b = matches_i[match_i];
        var match_c = matches_i[match_i + 1];
        if ("" == match_b.B || "" == match_b.P ||
            "" == match_c.B || "" == match_c.P) {
            continue;
        }
        var pair = null;
        pairs.some(function (element, index, array) {
            if (element.MatchB.B == match_b.B && element.MatchB.P == match_b.P &&
                element.MatchC.B == match_c.B && element.MatchC.P == match_c.P) {
                pair = element;
                return true;
           }
           return false;
        });
        if (null != pair) {
            pair.Count++;
        } else {
            pairs.push({
                Count: 1,
                MatchB: match_b,
                MatchC: match_c
            });
        }
    }
}
pairs.forEach(function (pair, index, array) {
    if (1 != pair.Count) {
        console.log(pair.MatchB.B + "/" + pair.MatchB.P + " > " + pair.MatchC.B + "/" + pair.MatchC.P + " = " + pair.Count);
    }
});*/
var pairs = [];
for (var time_i = 0; time_i < times.length; time_i++) {
    var matches_i = times[time_i].Matches;
    for (var match_i = 0; match_i < matches_i.length - 1; match_i++) {
        var match_b = matches_i[match_i];
        var match_c = matches_i[match_i + 1];
        if ("" == match_b.B || "" == match_b.P ||
            "" == match_c.B || "" == match_c.P) {
            continue;
        }
        var pair = null;
        var subBP = parseInt(match_c.B) - parseInt(match_c.P);
        if (pairs.some(function (element, index, array) {
            if (element.Match.B == match_b.B && element.Match.P == match_b.P) {
                pair = element;
                return true;
            }
        })) {
            pair.W += subBP > 0 ? 1 : 0;
            pair.L += subBP < 0 ? 1 : 0;
            pair.T += subBP == 0 ? 1 : 0;
        } else {
            pairs.push({
                W: subBP > 0 ? 1 : 0,
                L: subBP < 0 ? 1 : 0,
                T: subBP == 0 ? 1 : 0,
                Match: match_b
            });
        }
    }
}
pairs.forEach(function (pair, index, array) {
    var total = pair.W + pair.L + pair.T; 
    var absWL = Math.abs(pair.W - pair.L);
    if (2 < total && 2 < absWL || true) {
        console.log(pair.Match.B + "\t" + pair.Match.P + "\t" + pair.W + "\t" + pair.L + "\t" + pair.T);
    }
});