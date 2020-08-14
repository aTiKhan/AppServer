import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withRouter } from "react-router";
import {
  Backdrop,
  Heading,
  Aside,
  Row,
  RowContent,
  RowContainer,
  Text,
  Link,
  Button,
  toastr
} from "asc-web-components";
import { withTranslation } from "react-i18next";
import { utils as commonUtils, api } from "asc-web-common";
import i18n from "./i18n";
import { ReactSVG } from 'react-svg'
import {
  StyledAsidePanel,
  StyledContent,
  StyledHeaderContent,
  StyledBody,
  StyledFooter
} from "../StyledPanels";
import { getFileIcon, getFolderIcon, canWebEdit, isImage, isSound, isVideo } from "../../../store/files/selectors";
import { fetchFiles, setMediaViewerData, setTreeFolders, setNewTreeFilesBadge, setNewRowItems } from '../../../store/files/actions';
import store from "../../../store/store";

const { changeLanguage } = commonUtils;

class NewFilesPanelComponent extends React.Component {
  constructor(props) {
    super(props);

    changeLanguage(i18n);

    this.state = { files: [] };
  }

  componentDidMount() {
    const { folderId, onLoading } = this.props;
    onLoading(true);
    api.files
      .getNewFiles(folderId[folderId.length - 1])
      .then(files =>
        this.setState({ files })
      )
      .catch((err) => toastr.error(err))
      .finally(() => onLoading(false));
  }

  getItemIcon = (item, isEdit) => {
    const extension = item.fileExst;
    const icon = extension
      ? getFileIcon(extension, 24)
      : getFolderIcon(item.providerKey, 24);

    return (
      <ReactSVG
        beforeInjection={(svg) => {
          svg.setAttribute("style", "margin-top: 4px");
          isEdit && svg.setAttribute("style", "margin-left: 24px");
        }}
        src={icon}
        loading={this.svgLoader}
      />
    );
  };

  onMarkAsRead = () => {
    const { folderId, onClose } = this.props;
    const markAsReadFiles = true;

    const folderIds = [];
    const fileIds = [];
    const itemsIds = [];

    for(let item of this.state.files) {
      itemsIds.push(`${item.id}`);
      if(item.fileExst) {
        fileIds.push(item.id);
      } else {
        folderIds.push(item.id);
      }
    }

    api.files
      .markAsRead(folderIds, fileIds)
      .then(() => {
        this.props.setNewTreeFilesBadge(true);
        this.setNewFilesCount(folderId, markAsReadFiles);
        this.props.setNewRowItems(itemsIds);
      })
      .catch(err => toastr.error(err))
      .finally(() => onClose());
  };

  onNewFilesClick = item => {
    const { onClose, /*onLoading,*/ folderId } = this.props;
    const folderIds = [];
    const fileId = [];
    const isFile = item.fileExst;

    isFile ? fileId.push(item.id) : folderIds.push(item.id);

    api.files.markAsRead(folderIds, fileId)
      .then(() => {
        this.props.setNewTreeFilesBadge(true);
        this.setNewFilesCount(folderId, false, item);
        this.onFilesClick(item);
      })
      .catch(err => toastr.error(err))
      .finally(() => {
        !isFile && onClose();
      });
  }

  onFilesClick = item => {
    const { id, fileExst, viewUrl } = item;
    const { filter, setMediaViewerData } = this.props;

    if (!fileExst) {
      fetchFiles(id, filter, store.dispatch)
        .catch(err => toastr.error(err))
    } else {
      if (canWebEdit(fileExst)) {
        return window.open(`./doceditor?fileId=${id}`, "_blank");
      }
      
      const isOpenMedia = isImage(fileExst) || isSound(fileExst) || isVideo(fileExst);
      if (isOpenMedia) {
        const mediaItem = { visible: true, id };
        setMediaViewerData(mediaItem);
        return;
      }

      return window.open(viewUrl, "_blank");
    }
  };

  setNewFilesCount = (folderPath, markAsReadAll, item) => {
    const { treeFolders, setTreeFolders, folders, files } = this.props;

    const data = treeFolders;
    let dataItem;

    const loop = (index, newData) => {
      dataItem = newData.find(x => x.id === folderPath[index]);
      if(index === folderPath.length - 1) {
        const rootItem = data.find(x => x.id === folderPath[0]);
        const newFilesCounter = dataItem.newItems ? dataItem.newItems : dataItem.new;
        rootItem.newItems = markAsReadAll ? rootItem.newItems - newFilesCounter : rootItem.newItems - 1;
        dataItem.newItems = markAsReadAll ? 0 : newFilesCounter - 1;
        this.props.setNewRowItems([`${item.id}`]);
        return;
      } else { loop(index + 1, dataItem.folders); }
    }

    if(folderPath.length > 1) {
      loop(0, data);
    } else {
      dataItem = data.find(x => x.id === +folderPath[0]);
      dataItem.newItems = markAsReadAll ? 0 : dataItem.newItems - 1;

      if(item && item.fileExst) {
        const fileItem = files.find(x => x.id === item.id && x.fileExst);
        if(fileItem) {
          fileItem.new = markAsReadAll ? 0 : fileItem.new - 1;
        } else {
          const filesFolder = folders.find(x => x.id === item.folderId);
          if(filesFolder) {
            filesFolder.new = markAsReadAll ? 0 : filesFolder.new - 1;
          }
        }
        this.props.setNewRowItems([`${item.id}`]);
      } else if(item && !item.fileExst) {
        const folderItem = folders.find(x => x.id === item.id && !x.fileExst);
        if(folderItem) {
          folderItem.new = markAsReadAll ? 0 : folderItem.new - 1;
        }
      }
    }
  
    setTreeFolders(data);
  }


  render() {
    //console.log("NewFiles panel render");
    const { t, visible, onClose } = this.props;
    const { files } = this.state;
    const zIndex = 310;

    return (
      <StyledAsidePanel visible={visible}>
        <Backdrop onClick={onClose} visible={visible} zIndex={zIndex} />
        <Aside className="header_aside-panel" visible={visible}>
          <StyledContent>
            <StyledHeaderContent className="files-operations-panel">
              <Heading size="medium" truncate>
                {t("NewFiles")}
              </Heading>
            </StyledHeaderContent>
            <StyledBody className="files-operations-body">
              <RowContainer useReactWindow manualHeight="83vh">
                {files.map((file) => {
                  const element = this.getItemIcon(file);
                  return (
                    <Row key={file.id} element={element}>
                      <RowContent onClick={this.onNewFilesClick.bind(this, file)}>
                        <Link
                          containerWidth="100%"
                          type="page"
                          fontWeight="bold"
                          color="#333"
                          isTextOverflow
                          truncate
                          title={file.title}
                          fontSize="14px"
                        >
                          {file.title}
                        </Link>
                        <></>
                        <Text fontSize="12px" containerWidth="auto">
                          {file.checked && t("ConvertInto")}
                        </Text>
                      </RowContent>
                    </Row>
                  );
                })}
              </RowContainer>
            </StyledBody>
            <StyledFooter>
              <Button
                label={t("MarkAsRead")}
                size="big"
                primary
                onClick={this.onMarkAsRead}
              />
              <Button
                className="sharing_panel-button"
                label={t("CloseButton")}
                size="big"
                onClick={onClose}
              />
            </StyledFooter>
          </StyledContent>
        </Aside>
      </StyledAsidePanel>
    );
  }
}

NewFilesPanelComponent.propTypes = {
  onClose: PropTypes.func,
  visible: PropTypes.bool,
};

const NewFilesPanelContainerTranslated = withTranslation()(
  NewFilesPanelComponent
);

const NewFilesPanel = (props) => (
  <NewFilesPanelContainerTranslated i18n={i18n} {...props} />
);

const mapStateToProps = (state) => {
  const { filter, files, folders, treeFolders, selectedFolder } = state.files
  return { filter, files, folders, treeFolders, selectedFolder };
};

export default connect(mapStateToProps, { setMediaViewerData, setTreeFolders, setNewTreeFilesBadge, setNewRowItems })(withRouter(NewFilesPanel));