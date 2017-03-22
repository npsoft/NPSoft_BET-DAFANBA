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

== Skype
1. (x - xanh, d - đỏ)
- x x x x x x x x x x x x x x x x x ....
- d d d d d d d d d d d d d d ....
2. x d x d x d x d x d x d x d ....
3. xxd xxd xxd xxd ...
4. xx dd xx dd xx ...
đó là các loại đơn giản nhất để nhận thấy
mà thường rất khó để duy trì khoảng 16 lượt chơi
mình bắt từ lượt chơi thứ 10
