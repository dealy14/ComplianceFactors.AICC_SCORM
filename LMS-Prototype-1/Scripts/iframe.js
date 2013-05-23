$(document).ready(function()
	{
	    // Set specific variable to represent all iframe tags.
		var iFrames = document.getElementById('content');

		// Resize heights.
		function iResize()
		{
			// Iterate through all iframes in the page.
			for (var i = 0, j = iFrames.length; i < j; i++)
			{
				// Set inline style to equal the body height of the iframed content.
				iFrames[i].style.height = iFrames[i].contentWindow.document.body.offsetHeight + 'px';
				iFrames[i].style.height = iFrames[i].contentWindow.document.body.offsetHeight + 'px';
			}
		}


		// For other good browsers.
		$('#content').load(function()
			{
				// Set inline style to equal the body height of the iframed content.
				this.style.height = this.contentWindow.document.body.offsetHeight + 'px';
				this.style.width = this.contentWindow.document.body.offsetWidth + 'px';
			}
		);
	}
);