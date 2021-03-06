import React from "react";
import { withRouter } from "react-router";
import ModalDialogContainer from "../ModalDialogContainer";
import ModalDialog from "@appserver/components/modal-dialog";
import Button from "@appserver/components/button";
import Text from "@appserver/components/text";
import Checkbox from "@appserver/components/checkbox";
import Scrollbar from "@appserver/components/scrollbar";
import { withTranslation } from "react-i18next";
import toastr from "studio/toastr";
import { inject, observer } from "mobx-react";

class DeleteDialogComponent extends React.Component {
  constructor(props) {
    super(props);

    const foldersList = [];
    const filesList = [];
    const selection = [];

    let i = 0;
    while (props.selection.length !== i) {
      if (!(props.isRootFolder && props.selection[i].providerKey)) {
        if (
          props.selection[i].access === 0 ||
          props.selection[i].access === 1 ||
          props.unsubscribe
        ) {
          const item = { ...props.selection[i], checked: true };
          selection.push(item);
          if (props.selection[i].fileExst) {
            filesList.push(item);
          } else {
            foldersList.push(item);
          }
        }
      }
      i++;
    }

    this.state = { foldersList, filesList, selection };
  }

  onDelete = () => {
    this.onClose();
    const { t, deleteAction } = this.props;
    const translations = {
      deleteOperation: t("Translations:DeleteOperation"),
      deleteFromTrash: t("Translations:DeleteFromTrash"),
      deleteSelectedElem: t("Translations:DeleteSelectedElem"),
    };

    const selection = this.state.selection.filter((f) => f.checked);

    if (!selection.length) return;

    deleteAction(translations, selection).catch((err) => toastr.error(err));
  };

  onUnsubscribe = () => {
    this.onClose();
    const { unsubscribeAction } = this.props;

    const selection = this.state.selection.filter((f) => f.checked);

    if (!selection.length) return;

    let filesId = [];
    let foldersId = [];

    selection.map((item) => {
      item.fileExst ? filesId.push(item.id) : foldersId.push(item.id);
    });

    unsubscribeAction(filesId, foldersId).catch((err) => toastr.error(err));
  };

  onChange = (event) => {
    const value = event.target.value.split("/");
    const fileType = value[0];
    const id = Number(value[1]);

    const newSelection = this.state.selection;

    if (fileType !== "undefined") {
      const selection = newSelection.find((x) => x.id === id && x.fileExst);
      selection.checked = !selection.checked;
      this.setState({ selection: newSelection });
    } else {
      const selection = newSelection.find((x) => x.id === id && !x.fileExst);
      selection.checked = !selection.checked;
      this.setState({ selection: newSelection });
    }
  };

  onClose = () => {
    this.props.setRemoveMediaItem(null);
    this.props.setDeleteDialogVisible(false);
  };

  render() {
    const { visible, t, isLoading, unsubscribe } = this.props;
    const { filesList, foldersList, selection } = this.state;

    const checkedSelections = selection.filter((x) => x.checked === true);

    const title = unsubscribe
      ? t("UnsubscribeTitle")
      : checkedSelections.length === 1
      ? checkedSelections[0].fileExst
        ? t("MoveToTrashOneFileTitle")
        : t("MoveToTrashOneFolderTitle")
      : t("MoveToTrashItemsTitle");

    const noteText = unsubscribe
      ? t("UnsubscribeNote")
      : checkedSelections.length === 1
      ? checkedSelections[0].fileExst
        ? t("MoveToTrashOneFileNote")
        : t("MoveToTrashOneFolderNote")
      : t("MoveToTrashItemsNote");

    const accuracy = 20;
    let filesHeight = 25 * filesList.length + accuracy + 8;
    let foldersHeight = 25 * foldersList.length + accuracy + 8;
    if (foldersList.length === 0) {
      foldersHeight = 0;
    }
    if (filesList.length === 0) {
      filesHeight = 0;
    }

    const height = filesHeight + foldersHeight;

    return (
      <ModalDialogContainer visible={visible} onClose={this.onClose}>
        <ModalDialog.Header>{title}</ModalDialog.Header>
        <ModalDialog.Body>
          <div className="modal-dialog-content">
            <Text className="delete_dialog-header-text">{noteText}</Text>
            <Scrollbar style={{ height, maxHeight: 330 }} stype="mediumBlack">
              {foldersList.length > 0 && (
                <Text isBold className="delete_dialog-text">
                  {t("Translations:Folders")}:
                </Text>
              )}
              {foldersList.map((item, index) => (
                <Checkbox
                  truncate
                  className="modal-dialog-checkbox"
                  value={`${item.fileExst}/${item.id}`}
                  onChange={this.onChange}
                  key={`checkbox_${index}`}
                  isChecked={item.checked}
                  label={item.title}
                />
              ))}

              {filesList.length > 0 && (
                <Text isBold className="delete_dialog-text">
                  {t("Translations:Files")}:
                </Text>
              )}
              {filesList.map((item, index) => (
                <Checkbox
                  truncate
                  className="modal-dialog-checkbox"
                  value={`${item.fileExst}/${item.id}`}
                  onChange={this.onChange}
                  key={`checkbox_${index}`}
                  isChecked={item.checked}
                  label={item.title}
                />
              ))}
            </Scrollbar>
          </div>
        </ModalDialog.Body>
        <ModalDialog.Footer>
          <Button
            className="button-dialog-accept"
            key="OkButton"
            label={
              unsubscribe ? t("UnsubscribeButton") : t("MoveToTrashButton")
            }
            size="medium"
            primary
            onClick={unsubscribe ? this.onUnsubscribe : this.onDelete}
            isLoading={isLoading}
          />
          <Button
            className="button-dialog"
            key="CancelButton"
            label={t("Common:CancelButton")}
            size="medium"
            onClick={this.onClose}
            isLoading={isLoading}
          />
        </ModalDialog.Footer>
      </ModalDialogContainer>
    );
  }
}

const DeleteDialog = withTranslation([
  "DeleteDialog",
  "Common",
  "Translations",
])(DeleteDialogComponent);

export default inject(
  ({ filesStore, selectedFolderStore, dialogsStore, filesActionsStore }) => {
    const { selection, isLoading } = filesStore;
    const { deleteAction, unsubscribeAction } = filesActionsStore;

    const {
      deleteDialogVisible: visible,
      setDeleteDialogVisible,
      removeMediaItem,
      setRemoveMediaItem,
      unsubscribe,
    } = dialogsStore;

    return {
      selection: removeMediaItem ? [removeMediaItem] : selection,
      isLoading,
      isRootFolder: selectedFolderStore.isRootFolder,
      visible,

      setDeleteDialogVisible,
      deleteAction,
      unsubscribeAction,
      unsubscribe,

      setRemoveMediaItem,
    };
  }
)(withRouter(observer(DeleteDialog)));
