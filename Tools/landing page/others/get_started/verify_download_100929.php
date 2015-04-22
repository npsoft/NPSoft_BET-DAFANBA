<?php

//if (session_id() == "") session_start();

$mailSubject ="[photobookmart] Download Form";
//$mailTo = "maevng@gmail.com";
$mailTo = "admin@photobookmart.com";


$mailBody ="<table width=\"500\" border=\"1\" cellpadding=\"3\" cellspacing=\"0\" bordercolor=\"#e7e7e7\" class=\"content\">";

$mailBody .="<tr><td valign=\"top\" colspan=\"2\"><strong>Download Form</strong></td></tr>";

$mailBody .="<tr><td valign=\"top\">First Name</td><td>". $_POST['fname'] . "</td></tr>";

$mailBody .="<tr><td valign=\"top\">Surname</td><td>". $_POST['surname'] . "</td></tr>";

$mailBody .="<tr><td>Email</td><td>". $_POST['email'] . "</td></tr>";

$mailBody .="<tr><td valign=\"top\">Version</td><td>". $_POST['OS'] . "</td></tr>";

$mailBody .="<tr><td valign=\"top\">Country</td><td>". $_POST['country'] . "</td></tr>";

$mailBody .="</table>";

$mailBody .="<br /><br />Form generated on: " . date(" Y-m-d,  G:i:s T");




$mailHeaders = "MIME-Version: 1.0\r\n Content-Type: TEXT/PLAIN";
$mailHeaders ="From: ". $_POST['email']. "\n";
$mailHeaders .= "Content-type: text/html; charset=utf-8\n";
$mailHeaders .= "<link href=\"http://www.mirnet.com.my/demo/borneoaqua_html/css/fonts.css\" rel=\"stylesheet\" type=\"text/css\" />";
mail($mailTo, $mailSubject,$mailBody, $mailHeaders);



////auto respond to downloader
$mailSubject ="Photobookmart Software Download";
$mailTo = $_POST['email'];
$mailBody2 ="Dear Valued Customer,\n\n";
$mailBody2 .="Welcome to photobookmart! Thank you for downloading photobook editor. You can now enjoy designing your very own personalized photo book. Do visit our website for more ideas on photo book and various book styles.\n\n";
$mailBody2 .="Our support team will be readily assist you throughout the process of your photo book project. Visit our user guide and FAQ for more helpful tips.\n\n";
$mailBody2 .="Photobookmart";
$mailHeaders2 = "MIME-Version: 1.0\r\n Content-Type: TEXT/PLAIN";
$mailHeaders2 .="From:admin@photobookmart.com";
$mailHeaders2 .= "Content-type: text/html; charset=utf-8\n";
mail($mailTo, $mailSubject, $mailBody2, $mailHeaders2);



//$_SESSION['OS'] = $_POST['OS'];

//if (session_id() == "") session_start();
//$selected_radio = $_SESSION['OS'];

//if ($selected_radio == 'Windows') {
//	echo '<script type="text/javascript">';
//	echo 'window.open("http://rapidshare.com/files/391975901/MyPhotoCreationLInstaller.exe.html");';
//	echo '</script>';
	
//} else if ($selected_radio == 'Macintosh') {
//	echo '<script type="text/javascript">';
//	echo 'window.open("http://rapidshare.com/files/391981004/MyPhotoCreationsL.app.zip.html");';
//	echo '</script>';
//}


header ('location: thanks.html');

?>