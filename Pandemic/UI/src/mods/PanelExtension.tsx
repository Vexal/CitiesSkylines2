export function extendPanel(child) {
	return (Component) => {
		return (props) => {
			const { children, ...otherProps } = props || {};
			return (
				<>
					<Component {...otherProps} >{children}</Component>
					{child}
				</>
			);
		}
	}
}