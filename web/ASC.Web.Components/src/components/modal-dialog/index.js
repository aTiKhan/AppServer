import React from "react";
import PropTypes from "prop-types";
import styled from "styled-components";
import Backdrop from "../backdrop";
import Aside from "../layout/sub-components/aside";
import { Text } from "../text";
import { desktop } from "../../utils/device";
import throttle from "lodash/throttle";

const Dialog = styled.div`
  position: relative;
  width: auto;
  max-width: 560px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  min-height: 100%;
`;

const Content = styled.div`
  position: relative;
  width: 100%;
  background-color: #fff;
  padding: 0 16px 16px;
`;

const Header = styled.div`
  display: flex;
  align-items: center;
  border-bottom: 1px solid #dee2e6;
`;

const HeaderText = styled(Text.ContentHeader)`
  max-width: 500px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
`;

const CloseButton = styled.a`
  cursor: pointer;
  position: absolute;
  right: 16px;
  top: 20px;
  width: 16px;
  height: 16px;

  &:before,
  &:after {
    position: absolute;
    left: 8px;
    content: " ";
    height: 16px;
    width: 1px;
    background-color: #d8d8d8;
  }
  &:before {
    transform: rotate(45deg);
  }
  &:after {
    transform: rotate(-45deg);
  }
`;

const Body = styled.div`
  position: relative;
  padding: 16px 0;
`;

const Footer = styled.div``;

class ModalDialog extends React.Component {
  constructor(props) {
    super(props);

    this.state = { displayType: this.getTypeByWidth() };

    this.getTypeByWidth = this.getTypeByWidth.bind(this);
    this.resize = this.resize.bind(this);
    this.popstate = this.popstate.bind(this);
    this.throttledResize = throttle(this.resize, 300);
  }

  getTypeByWidth() {
    if (this.props.displayType !== "auto") return this.props.displayType;

    return window.innerWidth < desktop.match(/\d+/)[0] ? "aside" : "modal";
  }

  resize() {
    if (this.props.displayType !== "auto") return;

    const type = this.getTypeByWidth();
    if (type === this.state.displayType) return;

    this.setState({ displayType: type });
  }

  popstate() {
    window.removeEventListener("popstate", this.popstate, false);
    this.props.onClose();
    window.history.go(1);
  }

  componentDidUpdate(prevProps) {
    if (this.props.displayType !== prevProps.displayType) {
      this.setState({ displayType: this.getTypeByWidth() });
    }
    if (this.props.visible && this.state.displayType === "aside") {
      window.addEventListener("popstate", this.popstate, false);
    }
  }

  componentDidMount() {
    window.addEventListener("resize", this.throttledResize);
  }

  componentWillUnmount() {
    window.removeEventListener("resize", this.throttledResize);
  }

  render() {
    const {
      visible,
      scale,
      headerContent,
      bodyContent,
      footerContent,
      onClose,
      zIndex
    } = this.props;

    return this.state.displayType === "modal" ? (
      <Backdrop visible={visible} zIndex={zIndex}>
        <Dialog>
          <Content>
            <Header>
              <HeaderText>{headerContent}</HeaderText>
              <CloseButton onClick={onClose}></CloseButton>
            </Header>
            <Body>{bodyContent}</Body>
            <Footer>{footerContent}</Footer>
          </Content>
        </Dialog>
      </Backdrop>
    ) : (
      <>
        <Backdrop visible={visible} onClick={onClose} zIndex={zIndex}/>
        <Aside visible={visible} scale={scale} zIndex={zIndex} className="modal-dialog-aside">
          <Content>
            <Header>
              <HeaderText>{headerContent}</HeaderText>
              {scale ? <CloseButton onClick={onClose}></CloseButton> : ""}
            </Header>
            <Body>{bodyContent}</Body>
            <Footer className="modal-dialog-aside-footer">{footerContent}</Footer>
          </Content>
        </Aside>
      </>
    );
  }
}

ModalDialog.propTypes = {
  visible: PropTypes.bool,
  displayType: PropTypes.oneOf(["auto", "modal", "aside"]),
  scale: PropTypes.bool,
  headerContent: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node
  ]),
  bodyContent: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node
  ]),
  footerContent: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node
  ]),
  onClose: PropTypes.func,
  zIndex: PropTypes.number
};

ModalDialog.defaultProps = {
  displayType: "auto",
  zIndex: 310
};

export default ModalDialog;
