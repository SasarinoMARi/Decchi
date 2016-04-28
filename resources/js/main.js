(function($) {
	'use strict'; // Start of use strict
	
	// Highlight the top nav as scrolling occurs
	$('body').scrollspy({ target: '.navbar-fixed-top', offset: 70 });
	
	// jQuery for page scrolling feature - requires jQuery Easing plugin
	$('a.page-scroll').bind('click', function(event) {
		var $anchor = $(this);
		$('html, body').stop().animate({ scrollTop: ($($anchor.attr('href')).offset().top) - 50 }, 1250, 'easeInOutExpo');
		event.preventDefault();
	});

	$('.lazy').lazyload({ effect : 'fadeIn' });

	// Closes the Responsive Menu on Menu Item Click
	$('.navbar-collapse .page-scroll').click(function() {
		$('.navbar-toggle:visible').click();
	});

	// Offset for Main Navigation
	$('#main-nav').affix({ offset: { top: 0 } });
	
	if (navigator.appVersion.indexOf("Win") != -1) {
		$(document).ready(function() {
			$.getJSON('https://api.github.com/repos/Usagination/Decchi/releases/latest').done(function(json) {
				$('#download').attr('href', json.assets[0].browser_download_url);
				$('#download').text('최신버전 다운로드');
				$('#download').attr('disabled', false);
				$('#download').tooltip({ title: json.tag_name, placement: 'top', animation: true });
			}).fail(function() {
				$('#download').text('x_x');
			});
		});
	}
	else {
		$('#download').text('Windows 에서만 사용가능해요');
	}

	$(document).ready(function() {
		$('[data-toggle="tooltip"]').tooltip();

		$(window).load(function() {
			if (location.hash != '')
				$('html, body').stop().animate({ scrollTop: ($(location.hash).offset().top) - 50 }, 1, 'easeInOutExpo');
		});
	});
})(jQuery); // End of use strict
