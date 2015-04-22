<?php
require_once('../script/recaptchalib.php');
$publickey = "6LdAzLkSAAAAADTOB9yrYaMDG_1-i0DtgO_n69fb"; // you got this from the signup page

?>


<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
	<meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
	<title>Track Your Order</title>
  <link rel="stylesheet" type="text/css" href="../css/formstyles.css">
  <link href="../css/style.css" rel="stylesheet" type="text/css" />
<style type="text/css">
img, div, a, input { behavior: url(../css/iepngfix.htc) }

/* Active page link */
body {
	background: none;
}

<!-- 
a.red { 
display:block; 
width:270px; 
line-height:30px; 
background-color:#fff; 
color:#000; 
text-align:left; 
text-decoration:none; 
cursor:default; 
} 

a.red:hover { 
background-color:#FFCDC9; 
color:#000; 
} 


.error {
	font-size: 10pt;
	color: #EE3300;
	font-weight: bold;
}


--> 
</style>
</head>
<body>

	<script src=../js/valiformdownload.js></script>
	<form action="verify_download.php" method="post" enctype="application/x-www-form-urlencoded" name="download" id="download" onSubmit="return check(this);">
    <table width="500" border="0" cellspacing="0" cellpadding="10" class="text">
	  <tr>
        <td colspan="3" class="pinktxt"><strong>Download Now</strong></td>
      </tr>
      <tr>
        <td colspan="3"><strong>Please key in your information to download:</strong></td>
      </tr>
      <tr>
	    <td>First Name</td>
        <td>:</td>
	    <td><input type="text" name="fname" value="<?=htmlspecialchars(@$fields["fname"])?>" /></td>
	  </tr>
	  <tr>
	    <td>Surname</td>
        <td>:</td>
	    <td>
	      <input type="text" name="surname" value="<?=htmlspecialchars(@$fields["surname"])?>" />     
	    </td>
	  </tr>
	  <tr>
	    <td>Email</td>
        <td>:</td>
	    <td><input type="text" size="30" name="email" value="<?=@$fields["email"]?>" /></td>
	  </tr>
      <tr>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td height="30" align="left" valign="middle"><a class="red" href="#">
        <input name="OS" value="Windows" id="Windows" type="radio" />
        <strong>Download Windows version</strong> (55MB)</td>
      </tr>
      <tr>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
        <td align="left" valign="middle"><a class="red" href="#">
        <!--<input name="OS" value="Macintosh" id="Mac" type="radio" />-->
        <strong>Download Mac Version</strong> (45MB) COMING SOON</td>
      </tr>
      <tr>
	    <td>&nbsp;</td>
        <td>&nbsp;</td>
	    <td><?php echo recaptcha_get_html($publickey);?></td>
	  </tr>
      <tr>
	    <td>&nbsp;</td>
        <td>&nbsp;</td>
	    <td><input value="Begin Download" name="submit" type="submit" onClick="return check();">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input value="Reset" name="reset" type="reset"></td>
	  </tr>
     
	  </table>

		

  </form>

</body>
</html>