import React from 'react'
import styled from 'styled-components'
import { Icons } from '../icons'
import { Text } from '../text'
import PropTypes from 'prop-types'


const StyledContent = styled.div.attrs((props) => ({
  isOpen: props.isOpen
}))`

color: #333;
display: ${props => props.isOpen ? 'block' : 'none'};
padding-top: 9px;
`;

const Arrow = styled(Icons.ArrowContentIcon).attrs((props) => ({
  isOpen: props.isOpen
}))`

  margin-right: 9px;
  margin-bottom: 5px;
  transform: ${props => props.isOpen && 'rotate(180deg)'};
`;

class ToggleContent extends React.Component {

  constructor(props) {
    super(props);

    this.state = {
      isOpen: this.props.isOpen
    };
  }

  toggleContent = (isOpen) => this.setState({ isOpen: isOpen });

  componentDidUpdate(prevProps) {
    if (this.props.isOpen !== prevProps.isOpen) {
      this.setState({ isOpen: this.props.isOpen });
    }
  };

  render() {
    return (
      <>
        <div onClick={() => { this.toggleContent(!this.state.isOpen) }}>
          <Arrow color="#333333" isfill={true} size='medium' isOpen={this.state.isOpen} />
          <Text.Headline tag='h2' isInline={true}>{this.props.label}</Text.Headline>
        </div>
        <StyledContent isOpen={this.state.isOpen}>{this.props.children}</StyledContent>
      </>
    )
  }
}

ToggleContent.propTypes = {
  isOpen: PropTypes.bool
}

ToggleContent.defaultProps = {
  isOpen: false,
  label: "Some label"
}

export default ToggleContent;
