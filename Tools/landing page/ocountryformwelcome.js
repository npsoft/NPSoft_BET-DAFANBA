
// using the previous code
function initCountry()	{

rc = new Array();
rc[0] = [["Malaysia","MY"]]
rc[1] = [["Australia","AU"]]
rc[2] = [["Singapore","SG"]]
rc[3] = [["Philippines","PH"]]
rc[4] = [["Thailand","TH"]]
rc[5] = [["United Kingdom","UK"]]
rc[6] = [["India","IN"]]
rc[7] = [["Hong Kong","HK"]]
rc[8] = [["Others","www.photobookmart.com"]]
 

}
 

 
function showURL(which)	{
	//alert(which.options[which.selectedIndex].value);
	var obj = document.Country;
	var i = which.selectedIndex;
	if (i>0) //exclude '- Select -' in CountryList
	{
		var value =  which.options[i].value;
		if (value != "undefined")
		{
			if (value == "MY")	{
				obj.weburl.value = "www.photobookmart.com.my";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/Malaysia.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "AU")	{
				obj.weburl.value = "www.photobookmart.com.au";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/Australia.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "SG")	{
				obj.weburl.value = "www.photobookmart.com.sg";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/Singapore.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "PH")	{
				obj.weburl.value = "www.photobookmart.com.ph";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/Philippines.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";	
			}	else if (value == "TH")	{
				obj.weburl.value = "www.photobookmartthailand.com";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;
				document.getElementById('icon_country').innerHTML = "<img src='images/Thailand.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "UK")	{
				obj.weburl.value = "www.photobookmart.co.uk";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;	
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/UnitedKingdom.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "IN")	{
				obj.weburl.value = "www.photobookmart.co.in";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;	
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/India.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";	
			}	else if (value == "HK")	{
				obj.weburl.value = "www.photobookmart.com.hk";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;	
				document.getElementById('icon_country').innerHTML = "<img src='terms/images/Hongkong.png' width='32' height='32' valign='bottom' />&nbsp;&nbsp;&nbsp;";
			}	else if (value == "Others")	{
				obj.weburl.value = "www.photobookmart.com";
				document.getElementById('text_country').innerHTML = "http://" + obj.weburl.value;	
				document.getElementById('icon_country').innerHTML = "&nbsp;&nbsp;&nbsp;";
			} else	{
				obj.weburl.value = which.options[i].value;
				document.getElementById('text_country').innerHTML = "http://" + which.options[i].value;
				//obj.clickhere.value = obj.weburl.value;
			}
		} else	{//if undefined
			obj.weburl.value = "www.photobookmart.com";
			document.getElementById('text_country').innerHTML = "";
			//obj.clickhere.value = "GO";
		}
	}
}
 
function goto()	{
	var obj = document.Country;
	//alert('Go to: '+obj.weburl.value);
	//window.location = "http://"+ obj.weburl.value;
	window.open("http://"+ obj.weburl.value,"_blank");
}