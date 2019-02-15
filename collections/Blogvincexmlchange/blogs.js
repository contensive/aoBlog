//
// Email Subscribe
//
jQuery(document).ready(function(){
	jQuery('#blogSidebarEmailSubscribe').click(function(){
		cj.remote({
			'method':'blogSubscribeByEmailHandler',
			'queryString':'email='+jQuery('#blogSubscribeEmail').val()+'&blogid='+jQuery('#blogId').val(),
			'callback':blogEmailSubscribeCallback
		});
		return false;
		})
});
function blogEmailSubscribeCallback(response) {
	if(response.substring(0,2)=='OK'){
		jQuery('#blogSidebarEmailCell .blogSidebarCellInputCaption').html('You are subscribed to this blog.');
		jQuery('#blogSidebarEmailCell .blogSidebarCellInput').remove();
		jQuery('#blogSidebarEmailCell .blogSidebarCellButton').remove();
	} else {
	}
}
//
//----------
//
function blogNewRow() {
		var RLTable = document.getElementById('UploadInsert' );
		var CountElement = document.getElementById('LibraryUploadCount' );
		var tCols,tRows,NewRowNumber;
		var RowCnt = RLTable.rows.length;
		var NewRow,NewRow2,NewRow3,NewRow4,NewRow5,NewCell;
		if ( RLTable ) {
			NewRowNumber = parseInt( CountElement.value )+1;
			CountElement.value = NewRowNumber;
			tRows = RLTable.getElementsByTagName("TR");
			//
			//  put in a rule so rows dont run together
			//
			NewRow = RLTable.insertRow(RowCnt);
			NewCell = NewRow.insertCell(0);
			NewCell.innerHTML='<hr>';
			//
			//  order
			//
			NewRow2 = RLTable.insertRow(RowCnt+1);
			NewCell = NewRow2.insertCell(0);
			NewCell.innerHTML= 'Order<br><INPUT TYPE="Text" NAME="LibraryOrder.'+NewRowNumber+'" SIZE="2" value="'+parseInt(NewRowNumber)+'">'
			//
			//  image
			//
			NewRow3 = RLTable.insertRow(RowCnt+2);
			NewCell = NewRow3.insertCell(0);
			NewCell.innerHTML='Image<br><INPUT TYPE="file" name="LibraryUpload.'+NewRowNumber+'">';
			//
			//  name
			//
			NewRow4 = RLTable.insertRow(RowCnt+3);
			NewCell = NewRow4.insertCell(0);
			NewCell.innerHTML= 'Name<br><INPUT TYPE="Text" NAME="LibraryName.'+NewRowNumber+'" SIZE="30">'
			//
			//  description
			//
			NewRow5 = RLTable.insertRow(RowCnt+4);
			NewCell = NewRow5.insertCell(0);
			NewCell.innerHTML='Description<br><TEXTAREA NAME="LibraryDescription.'+NewRowNumber+'" ROWS="5" COLS="50"></TEXTAREA>';
		}
}
