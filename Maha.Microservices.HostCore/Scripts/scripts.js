
$(document).ready(function(){/* activate sidebar */
$('#sidebar').affix({
  offset: {
    top: 235
  }
});

/* activate scrollspy menu */
var $body   = $(document.body);
var navHeight = $('.navbar').outerHeight(true) + 10;

$body.scrollspy({
	target: '#leftCol',
	offset: navHeight
});

/* smooth scrolling sections */
$('a[href*=#]:not([href=#])').click(function() {
    if (location.pathname.replace(/^\//,'') == this.pathname.replace(/^\//,'') && location.hostname == this.hostname) {
      var target = $(this.hash);
      target = target.length ? target : $('[name=' + this.hash.slice(1) +']');
      if (target.length) {
        $('html,body').animate({
          scrollTop: target.offset().top - 50
        }, 1000);
        return false;
      }
    }
});
});

/* 
* ���ʱ���,ʱ���ʽΪ ��-��-�� Сʱ:����:�� ���� ��/��/�� Сʱ�����ӣ��� 
* ���У�������Ϊȫ��ʽ������ �� 2010-10-12 01:00:00 
* ���ؾ���Ϊ�����룬�룬�֣�Сʱ���� 
*/
function GetDateDiff(startTime, endTime, diffType) {
    var sTime = new Date(startTime); //��ʼʱ�� 
    var eTime = new Date(endTime); //����ʱ�� 
    //��Ϊ���������� 
    var divNum = 1;
    switch (diffType) {
        case "ms":
            divNum = 1;
            break;
        case "second":
            divNum = 1000;
            break;
        case "minute":
            divNum = 1000 * 60;
            break;
        case "hour":
            divNum = 1000 * 3600;
            break;
        case "day":
            divNum = 1000 * 3600 * 24;
            break;
        default:
            break;
    }
    return parseInt((eTime.getTime() - sTime.getTime()) / parseInt(divNum));
}

function formatJSON(data) {
    try {
        return JSON.stringify(JSON.parse(data), null, "    ");
    } catch (e) {
        return data;
    }
}